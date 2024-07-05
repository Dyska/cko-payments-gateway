namespace PaymentGateway.Api.Models.Responses;

public class CardResponse : Card
{
    public required string CardNumberFinalFourDigits { get; init; }
};