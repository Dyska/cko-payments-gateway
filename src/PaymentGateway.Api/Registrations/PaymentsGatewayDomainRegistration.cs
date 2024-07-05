using PaymentGateway.Api.Domain.Clients.Bank;
using PaymentGateway.Api.Domain.Repositories;
using PaymentGateway.Api.Domain.Services;

namespace PaymentGateway.Api.Registrations;

public static class PaymentsGatewayDomainRegistration
{
    public static IServiceCollection AddPaymentsGatewayDomain(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentRepository, InMemoryPaymentRepository>();
        services.AddScoped<IBankClient, ImposterBankClient>();

        return services;
    }
}