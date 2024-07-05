using PaymentGateway.Api.Domain.Models;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Mappers;

public static class PaymentGatewayMapper
{
    public static Payment ToPayment(this ProcessPaymentRequest requestBody)
    {
        var cardRequest = requestBody.Card;
        var card = new Card(cardRequest.ExpiryMonth, cardRequest.ExpiryYear, cardRequest.CardNumber, cardRequest.CVV);

        return new Payment(card, requestBody.ISOCurrencyCode, requestBody.Amount);
    }
}