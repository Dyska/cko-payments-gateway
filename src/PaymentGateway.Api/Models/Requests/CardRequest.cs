namespace PaymentGateway.Api.Models.Requests;

public class CardRequest : Card
{
    public required string CardNumber { get; init; }
    public required string CVV { get; init; }
};