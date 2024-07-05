namespace PaymentGateway.Api.Models;

public abstract class Card
{
    public required string ExpiryMonth { get; init; }
    public required string ExpiryYear { get; init; }
};
