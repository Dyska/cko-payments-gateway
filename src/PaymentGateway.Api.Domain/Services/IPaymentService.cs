using PaymentGateway.Api.Domain.Models;

namespace PaymentGateway.Api.Domain.Services;

public interface IPaymentService
{
    public Task<Payment?> FetchPayment(Guid paymentId);
    public Task<Payment> ProcessPayment(Payment payment);
}