using PaymentGateway.Api.Domain.Models;

namespace PaymentGateway.Api.Domain.Services;

public interface IPaymentService
{
    public Task<Payment?> ProcessPayment(Payment payment);
}