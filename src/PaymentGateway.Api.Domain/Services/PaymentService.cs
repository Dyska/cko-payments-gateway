using PaymentGateway.Api.Domain.Clients.Bank;
using PaymentGateway.Api.Domain.Models;
using PaymentGateway.Api.Domain.Repositories;

namespace PaymentGateway.Api.Domain.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBankClient _bankClient;

    public PaymentService(IPaymentRepository paymentRepository, IBankClient bankClient)
    {
        _paymentRepository = paymentRepository;
        _bankClient = bankClient;
    }

    public async Task<Payment?> FetchPayment(Guid paymentId)
    {
        return await _paymentRepository.GetPayment(paymentId);
    }

    public async Task<Payment> ProcessPayment(Payment payment)
    {
        await _paymentRepository.SavePayment(payment);

        var bankPaymentRequest = payment.ToBankPaymentRequest();
        var bankPaymentResponse = await _bankClient.SubmitPayment(bankPaymentRequest);

        if (bankPaymentResponse.Authorized)
        {
            payment.SetSuccessful(bankPaymentResponse.AuthorizationCode!.Value);
        }
        else
        {
            payment.SetDeclined();
        }

        await _paymentRepository.UpdatePayment(payment);
        return payment;
    }
}