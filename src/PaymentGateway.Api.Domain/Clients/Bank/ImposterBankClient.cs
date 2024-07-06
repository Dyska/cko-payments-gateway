using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Domain.Clients.Bank.Models;

namespace PaymentGateway.Api.Domain.Clients.Bank;

public class ImposterBankClient : IBankClient
{
    private readonly ILogger<ImposterBankClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _options;

    public ImposterBankClient(ILogger<ImposterBankClient> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient(); //This approach will not scale
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };
    }

    public async Task<PaymentResponse> SubmitPayment(PaymentRequest paymentRequest)
    {
        //Note: Would read from AppSettings + Options
        string url = "http://localhost:8080/payments";

        string jsonContent = JsonSerializer.Serialize(paymentRequest, _options);
        StringContent content = new(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            PaymentResponse? paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseBody, _options);
            return paymentResponse ?? new PaymentResponse(false);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning($"Request to submit payment to bank failed: {e.Message}");
            return new PaymentResponse(false);
        }
    }
}