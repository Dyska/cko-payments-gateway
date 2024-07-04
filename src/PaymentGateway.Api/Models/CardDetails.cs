namespace PaymentGateway.Api.Models;

public class CardDetails
{
    public required string FinalFourCardDigits { get; set; } //Validation - should be 4 characters, all numbers
    public required string ExpiryMonth { get; set; }
    public required string ExpiryYear {get; set; }
};