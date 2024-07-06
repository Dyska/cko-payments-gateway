using PaymentGateway.Api.Domain.Models;

namespace PaymentGateway.Api.UnitTests.Helpers;

public static class TestPaymentFactory
{
    public static Payment GenerateValidPayment()
    {
        var validCard = new Card("03", "2025", "2222405343248877", "354");
        var validCurrency = "NZD";
        var inputAmount = 1000m;

        return new Payment(validCard, validCurrency, inputAmount);
    }
}