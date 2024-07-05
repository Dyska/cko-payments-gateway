namespace PaymentGateway.Api.Models.Responses;

public abstract class PaymentDetailsResponse
{
    public required Guid Id { get; init; }
    public required string Status { get; init; }
    public required Card CardDetails { get; init; }
    public required string ISOCurrencyCode { get; init; }
    public required decimal Amount { get; init; }
}