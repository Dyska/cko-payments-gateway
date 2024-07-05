namespace PaymentGateway.Api.Domain.Clients.Bank.Models;

public class PaymentResponse(bool authorized, Guid? authorizationCode = null)
{
    public bool Authorized { get; init; } = authorized;
    public Guid? AuthorizationCode { get; init; } = authorizationCode;
}