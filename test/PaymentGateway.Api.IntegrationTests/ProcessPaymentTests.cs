using System.Net;
using System.Net.Http.Json;
using System.Text;

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
        var emptyContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync($"/api/v1/payments", emptyContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPayment_FieldsMissingFromBody_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var paymentBodyMissingAmount = new {
            card = new {
                expiryMonth = "01",
                expiryYear = "2026",
                cardNumber = "2222405343248877",
                cvv = "234"
            },
            isoCurrencyCode = "NZD",
        };

        // Act
        var response = await client.PostAsync($"/api/v1/payments", JsonContent.Create(paymentBodyMissingAmount));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPayment_PaymentFailsValidation_ReturnsBadRequestWithRejectedStatus()
    {
        // Arrange
        var client = _factory.CreateClient();
        var paymentBodyUnsupportedCurrency = new {
            card = new {
                expiryMonth = "01",
                expiryYear = "2026",
                cardNumber = "2222405343248877",
                cvv = "234"
            },
            isoCurrencyCode = "AUD",
            amount = 10000,
        };

        // Act
        var response = await client.PostAsync($"/api/v1/payments", JsonContent.Create(paymentBodyUnsupportedCurrency));

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
        var paymentBody = new {
            card = new {
                expiryMonth = "04",
                expiryYear = "2025",
                cardNumber = "2222405343248877",
                cvv = "123"
            },
            isoCurrencyCode = "GBP",
            amount = 100,
        };

        // Act
        var response = await client.PostAsync($"/api/v1/payments", JsonContent.Create(paymentBody));

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
    [InlineData("456")]
    [InlineData("457")]
    public async Task ProcessPayment_PaymentRejectedByBank_ReturnsCreatedWithDeclinedStatus(string cvv)
    {
        // Arrange
        // cvv of 456 will return a 200, Rejected response from Bank client
        // cvv of 457 will return a 400 response from Bank client, which is mapped to Rejected

        var client = _factory.CreateClient();
        var paymentBody = new {
            card = new {
                expiryMonth = "01",
                expiryYear = "2026",
                cardNumber = "2222405343248112",
                cvv = cvv
            },
            isoCurrencyCode = "USD",
            amount = 60000,
        };

        // Act
        var response = await client.PostAsync($"/api/v1/payments", JsonContent.Create(paymentBody));

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

    //Check duplicate requests trigger failure due to idempotency
}