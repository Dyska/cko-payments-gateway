namespace PaymentGateway.Api.Models;

public abstract class Card
{
    public required string ExpiryMonth { get; init; }
    public required string ExpiryYear { get; init; }
};

public class CardResponse : Card
{
    public required string CardNumberFinalFourDigits { get; init; } 
};

public class CardRequest : Card
{
    public required string CardNumber { get; init; }
    public required string CVV { get; init; } 
};