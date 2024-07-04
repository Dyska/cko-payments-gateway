using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentGatewayController : ControllerBase
    {

        private readonly ILogger<PaymentGatewayController> _logger;

        public PaymentGatewayController(ILogger<PaymentGatewayController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}", Name = "GetPaymentDetails")]
        [ProducesResponseType<GetPaymentDetailsResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPaymentDetails([FromRoute] Guid id)
        {
            return Ok(new GetPaymentDetailsResponse {
                Id = id,
                Status = PaymentRequestStatus.Authorized,
                CardDetails = new CardDetails {
                    FinalFourCardDigits = "1234",
                    ExpiryMonth = "01",
                    ExpiryYear = "2025",
                },
                ISOCurrencyCode = "NZD",
                AmountMinorUnit = 10000,
            });
        }

        // [HttpPost(Name = "ProcessPayment")]
        // public IEnumerable<WeatherForecast> ProcessPayment()
        // {
        //     return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //     {
        //         Date = DateTime.Now.AddDays(index),
        //         TemperatureC = Random.Shared.Next(-20, 55),
        //         Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //     })
        //     .ToArray();
        // }
    }
}
