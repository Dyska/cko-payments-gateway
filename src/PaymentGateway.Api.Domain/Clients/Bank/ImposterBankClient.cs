using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Domain.Clients.Bank.Models;

namespace PaymentGateway.Api.Domain.Clients.Bank;

public class ImposterBankClient : IBankClient
{
    //TODO: Read in URI from options
    private readonly HttpClient _httpClient; //TODO: What should scoping of http client be - singleton?
    private readonly JsonSerializerOptions _options;
    private readonly ILogger<ImposterBankClient> _logger;

    public ImposterBankClient(ILogger<ImposterBankClient> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient(); //TODO: DI?
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };
    }

    public async Task<PaymentResponse> SubmitPayment(PaymentRequest paymentRequest)
    {
        string url = "http://localhost:8080/payments";

        string jsonContent = JsonSerializer.Serialize(paymentRequest, _options);
        StringContent content = new(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            PaymentResponse? paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseBody);
            return paymentResponse ?? new PaymentResponse(false);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning($"Request error: {e.Message}");
            return new PaymentResponse(false);
        }
    }
}