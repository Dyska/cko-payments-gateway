using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Moq;

using PaymentGateway.Api.Domain.Models;
using PaymentGateway.Api.Domain.Repositories;

namespace PaymentGateway.Api.IntegrationTests;

public class GetPaymentTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GetPaymentTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPayment_WhenPaymentNotInDatabase_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        Guid validGuid = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/v1/payments/{validGuid}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPayment_WhenPaymentInDatabase_ReturnsOK()
    {
        var validCard = new Card("03", "2025", "2222405343248877", "354");
        var validCurrency = "NZD";
        var inputAmount = 1000m;
        var payment = new Payment(validCard, validCurrency, inputAmount);
        var authorizationCode = Guid.NewGuid();
        payment.SetSuccessful(authorizationCode);

        var paymentId = payment.Id;

        var paymentRepositoryMock = new Mock<IPaymentRepository>();
        paymentRepositoryMock.Setup(p => p.GetPayment(It.IsAny<Guid>())).ReturnsAsync(payment);

        //Wouldn't typically have to do this - if we want to ensure data exists in DB,
        //we could insert it ahead of time, or use something like EF Core's
        //in memory database
        //Here there is no simple mechanism other than mocking response
        var modifiedFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IPaymentRepository>();
                services.AddSingleton(paymentRepositoryMock.Object);
            });
        });

        var client = modifiedFactory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/payments/{paymentId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string contentString = await response.Content.ReadAsStringAsync();

        Assert.Contains($"\"id\":\"{paymentId}\"", contentString);
        Assert.Contains("\"status\":\"Authorized\"", contentString);
        Assert.Contains("\"expiryMonth\":\"03\"", contentString);
        Assert.Contains("\"expiryYear\":\"2025\"", contentString);
        Assert.Contains("\"cardNumberFinalFourDigits\":\"8877\"", contentString);
        Assert.Contains("\"isoCurrencyCode\":\"NZD\"", contentString);
        Assert.Contains("\"amount\":1000", contentString);
    }
}