using PaymentGateway.Api.Domain.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Mappers;

public static class PaymentGatewayMapper
{
    public static Payment ToPayment(this ProcessPaymentRequest requestBody)
    {
        var cardRequest = requestBody.Card;
        var card = new Card(cardRequest.ExpiryMonth, cardRequest.ExpiryYear, cardRequest.CardNumber, cardRequest.CVV);

        return new Payment(card, requestBody.ISOCurrencyCode, requestBody.Amount);
    }

    public static ProcessPaymentResponse ToProcessPaymentResponse(this Payment response)
    {
        return new ProcessPaymentResponse()
        {
            Id = response.Id,
            Status = response.Status.ToString(),
            CardDetails = new CardResponse
            {
                CardNumberFinalFourDigits = GetLastFourDigits(response.Card.CardNumber),
                ExpiryMonth = response.Card.ExpiryMonth,
                ExpiryYear = response.Card.ExpiryYear,
            },
            ISOCurrencyCode = response.ISOCurrencyCode,
            Amount = response.Amount,
        };
    }

    public static GetPaymentDetailsResponse? ToGetPaymentDetailsResponse(this Payment? response)
    {
        if (response == null)
        {
            return null;
        }
        return new GetPaymentDetailsResponse()
        {
            Id = response.Id,
            Status = response.Status.ToString(),
            CardDetails = new CardResponse
            {
                CardNumberFinalFourDigits = GetLastFourDigits(response.Card.CardNumber),
                ExpiryMonth = response.Card.ExpiryMonth,
                ExpiryYear = response.Card.ExpiryYear,
            },
            ISOCurrencyCode = response.ISOCurrencyCode,
            Amount = response.Amount,
        };
    }

    private static string GetLastFourDigits(string cardNumber)
    {
        return (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4) ? "" : cardNumber.Substring(cardNumber.Length - 4);
    }
}