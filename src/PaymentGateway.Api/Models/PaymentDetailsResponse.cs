using System.Numerics;

namespace PaymentGateway.Api.Models
{
    public abstract class PaymentDetailsResponse
    {
        public required Guid Id { get; init; }
        public required PaymentRequestStatus Status { get; init; }
        public required Card CardDetails { get; init; }
        public required string ISOCurrencyCode { get; init; }
        public required BigInteger Amount { get; init;}
    }
}