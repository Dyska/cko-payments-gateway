using PaymentGateway.Api.Domain.Models;

namespace PaymentGateway.Api.Domain.Repositories;

public interface IPaymentRepository
{
    public Task<Payment?> GetPayment(Guid paymentId);

    public Task<bool> SavePayment(Payment payment);

    public Task<bool> UpdatePayment(Payment payment);
}