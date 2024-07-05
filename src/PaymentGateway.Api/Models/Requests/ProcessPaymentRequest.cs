namespace PaymentGateway.Api.Models.Requests
{
    public class ProcessPaymentRequest
    {
        public required CardRequest Card { get; init; }
        public required string ISOCurrencyCode { get; init; }
        public required decimal Amount { get; init; }
    }
}