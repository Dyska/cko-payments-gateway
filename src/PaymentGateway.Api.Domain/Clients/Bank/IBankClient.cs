using PaymentGateway.Api.Domain.Clients.Bank.Models;

namespace PaymentGateway.Api.Domain.Clients.Bank;

public interface IBankClient
{
    public Task<PaymentResponse> SubmitPayment(PaymentRequest paymentRequest);
}