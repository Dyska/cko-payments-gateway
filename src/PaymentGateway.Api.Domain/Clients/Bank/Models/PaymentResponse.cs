namespace PaymentGateway.Api.Domain.Clients.Bank.Models;

public class PaymentResponse()
{
    public bool Authorized { get; init; }
    public Guid? AuthorizationCode { get; init; }
}