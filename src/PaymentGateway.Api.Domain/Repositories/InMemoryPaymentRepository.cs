using System.Collections.Concurrent;

using PaymentGateway.Api.Domain.Models;

namespace PaymentGateway.Api.Domain.Repositories;

public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly ConcurrentDictionary<Guid, Payment> _inMemoryStorage = [];

    public Task<Payment?> GetPayment(Guid paymentId)
    {
        return Task.FromResult(_inMemoryStorage.TryGetValue(paymentId, out Payment? value) ? value : null);
    }

    public Task<bool> SavePayment(Payment payment)
    {
        return Task.FromResult(_inMemoryStorage.TryAdd(payment.Id, payment));
    }

    public Task<bool> UpdatePayment(Payment payment)
    {
        bool didUpdate = false;
        if (_inMemoryStorage.ContainsKey(payment.Id))
        {
            _inMemoryStorage[payment.Id] = payment;
            didUpdate = true;
        }
        return Task.FromResult(didUpdate);
    }
}