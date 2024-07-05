namespace PaymentGateway.Api.Models
{
    public class ProcessPaymentRequestBody
    {
        public required CardRequest Card { get; init; }
        public required string ISOCurrencyCode { get; init; }
        public required decimal Amount { get; init; }
    }
}