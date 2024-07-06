using PaymentGateway.Api.Domain.Clients.Bank;
using PaymentGateway.Api.Domain.Repositories;
using PaymentGateway.Api.Domain.Services;

namespace PaymentGateway.Api.Registrations;

public static class PaymentsGatewayDomainRegistration
{
    public static IServiceCollection AddPaymentsGatewayDomain(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IBankClient, ImposterBankClient>();

        //Note: Singleton only makes sense here because this object needs to persist between requests
        //A traditional repository would be scoped or transient 
        services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();

        return services;
    }
}