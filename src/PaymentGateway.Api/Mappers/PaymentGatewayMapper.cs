using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Domain.Models;

namespace PaymentGateway.Api.Mappers
{
    public static class PaymentGatewayMapper
    {
        public static Payment ToPayment(this ProcessPaymentRequest requestBody)
        {
            var cardRequest = requestBody.Card;
            var card = new Domain.Models.Card(cardRequest.ExpiryMonth, cardRequest.ExpiryYear, cardRequest.CardNumber, cardRequest.CVV);

            return new Payment(card, requestBody.ISOCurrencyCode, requestBody.Amount);
        }
    }
}