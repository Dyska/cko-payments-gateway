using System.Numerics;

namespace PaymentGateway.Api.Models
{
    public class GetPaymentDetailsResponse
    {
        public required Guid Id { get; set; }
        public required PaymentRequestStatus Status { get; set; }
        public required CardDetails CardDetails { get; set; }
        public required string ISOCurrencyCode {get; set; }
        public required BigInteger AmountMinorUnit {get; set;}
    };
}