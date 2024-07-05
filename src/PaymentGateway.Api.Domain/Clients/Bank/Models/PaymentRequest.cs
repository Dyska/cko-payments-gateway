namespace PaymentGateway.Api.Domain.Clients.Bank.Models;

public class PaymentRequest
{
    public required string CardNumber { get; init; }
    public required string ExpiryDate { get; init; }
    public required string Currency { get; init; }
    public required decimal Amount { get; init; } //TODO: decimal?
    public required string CVV { get; init; }
}