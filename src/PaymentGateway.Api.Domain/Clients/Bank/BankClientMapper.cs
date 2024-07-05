using PaymentGateway.Api.Domain.Clients.Bank.Models;
using PaymentGateway.Api.Domain.Models;

namespace PaymentGateway.Api.Domain.Clients.Bank;

public static class BankClientMapper
{
    public static PaymentRequest ToBankPaymentRequest(this Payment payment)
    {
        return new PaymentRequest()
        {
            CardNumber = payment.Card.CardNumber,
            ExpiryDate = $"{payment.Card.ExpiryMonth}/{payment.Card.ExpiryYear}",
            Currency = payment.ISOCurrencyCode,
            Amount = payment.Amount,
            CVV = payment.Card.CVV,
        };
    }
}