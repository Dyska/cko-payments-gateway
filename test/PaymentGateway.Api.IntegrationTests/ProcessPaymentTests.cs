using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

namespace PaymentGateway.Api.IntegrationTests;

public class ProcessPaymentTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProcessPaymentTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProcessPayment_EmptyBody_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments")
        {
            Content = JsonContent.Create(string.Empty)
        };
        request.Headers.Add("Idempotency-Token", Guid.NewGuid().ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPayment_FieldsMissingFromBody_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var paymentBodyMissingAmount = new
        {
            card = new
            {
                expiryMonth = "01",
                expiryYear = "2026",
                cardNumber = "2222405343248877",
                cvv = "234"
            },
            isoCurrencyCode = "NZD",
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments")
        {
            Content = JsonContent.Create(paymentBodyMissingAmount)
        };
        request.Headers.Add("Idempotency-Token", Guid.NewGuid().ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPayment_PaymentFailsValidation_ReturnsBadRequestWithRejectedStatus()
    {
        // Arrange
        var client = _factory.CreateClient();
        var paymentBodyUnsupportedCurrency = new
        {
            card = new
            {
                expiryMonth = "01",
                expiryYear = "2026",
                cardNumber = "2222405343248877",
                cvv = "234"
            },
            isoCurrencyCode = "AUD",
            amount = 10000,
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments")
        {
            Content = JsonContent.Create(paymentBodyUnsupportedCurrency)
        };
        request.Headers.Add("Idempotency-Token", Guid.NewGuid().ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        string contentString = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"Rejected\"", contentString);
    }

    [Fact]
    public async Task ProcessPayment_PaymentAuthorisedByBank_ReturnsCreatedWithAuthorizedStatus()
    {
        // Arrange
        var client = _factory.CreateClient();
        var paymentBody = new
        {
            card = new
            {
                expiryMonth = "04",
                expiryYear = "2025",
                cardNumber = "2222405343248877",
                cvv = "123"
            },
            isoCurrencyCode = "GBP",
            amount = 100,
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments")
        {
            Content = JsonContent.Create(paymentBody)
        };
        request.Headers.Add("Idempotency-Token", Guid.NewGuid().ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        string contentString = await response.Content.ReadAsStringAsync();

        string pattern = "\"id\":\"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\"";
        Assert.Matches(pattern, contentString);

        Assert.Contains("\"status\":\"Authorized\"", contentString);
        Assert.Contains("\"expiryMonth\":\"04\"", contentString);
        Assert.Contains("\"expiryYear\":\"2025\"", contentString);
        Assert.Contains("\"cardNumberFinalFourDigits\":\"8877\"", contentString);
        Assert.Contains("\"isoCurrencyCode\":\"GBP\"", contentString);
        Assert.Contains("\"amount\":100", contentString);
    }

    [Theory]
    [InlineData("456")] // cvv of 456 will return a 200, Rejected response from Bank client
    [InlineData("457")] // cvv of 457 will return a 400 response from Bank client, which is also mapped to Rejected
    public async Task ProcessPayment_PaymentRejectedByBank_ReturnsCreatedWithDeclinedStatus(string cvv)
    {
        // Arrange
        var client = _factory.CreateClient();
        var paymentBody = new
        {
            card = new
            {
                expiryMonth = "01",
                expiryYear = "2026",
                cardNumber = "2222405343248112",
                cvv = cvv
            },
            isoCurrencyCode = "USD",
            amount = 60000,
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments")
        {
            Content = JsonContent.Create(paymentBody)
        };
        request.Headers.Add("Idempotency-Token", Guid.NewGuid().ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        string contentString = await response.Content.ReadAsStringAsync();

        string pattern = "\"id\":\"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\"";
        Assert.Matches(pattern, contentString);

        Assert.Contains("\"status\":\"Declined\"", contentString);
        Assert.Contains("\"expiryMonth\":\"01\"", contentString);
        Assert.Contains("\"expiryYear\":\"2026\"", contentString);
        Assert.Contains("\"cardNumberFinalFourDigits\":\"8112\"", contentString);
        Assert.Contains("\"isoCurrencyCode\":\"USD\"", contentString);
        Assert.Contains("\"amount\":60000", contentString);
    }

    [Fact]
    public async Task ProcessPayment_DuplicateRequests_ReturnsTooManyRequests()
    {
        // Arrange
        var client = _factory.CreateClient();
        string sharedIdempotencyHeader = Guid.NewGuid().ToString();

        var paymentBody = new
        {
            card = new
            {
                expiryMonth = "04",
                expiryYear = "2025",
                cardNumber = "2222405343248877",
                cvv = "123"
            },
            isoCurrencyCode = "GBP",
            amount = 100,
        };

        var firstRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments")
        {
            Content = JsonContent.Create(paymentBody)
        };
        var secondRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments")
        {
            Content = JsonContent.Create(paymentBody)
        };

        firstRequest.Headers.Add("Idempotency-Token", sharedIdempotencyHeader);
        secondRequest.Headers.Add("Idempotency-Token", sharedIdempotencyHeader);

        // Act
        var _ = await client.SendAsync(firstRequest); //First request should be fine
        var response = await client.SendAsync(secondRequest);

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }
}