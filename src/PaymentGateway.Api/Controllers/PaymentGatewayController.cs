using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Domain.Models;
using PaymentGateway.Api.Mappers;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
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
                CardDetails = new CardResponse {
                    CardNumberFinalFourDigits = "1234",
                    ExpiryMonth = "01",
                    ExpiryYear = "2025",
                },
                ISOCurrencyCode = "NZD",
                Amount = 10000,
            });
        }

        [HttpPost(Name = "ProcessPayment")]
        [ProducesResponseType<ProcessPaymentResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ProcessPayment([FromBody] ProcessPaymentRequestBody body)
        {
            //Don't forget idempotency token!

            Payment payment;
            try {
                payment = body.ToPayment();
            } catch (ArgumentException ex) {
                return BadRequest(new { Status = PaymentRequestStatus.Declined, Mesage = ex.Message});
            }

            return Created(
                "", //TODO: Construct URI for response
                new ProcessPaymentResponse {
                Id = Guid.NewGuid(),
                Status = PaymentRequestStatus.Authorized,
                CardDetails = new CardResponse {
                    CardNumberFinalFourDigits = "1234",
                    ExpiryMonth = "01",
                    ExpiryYear = "2025",
                },
                ISOCurrencyCode = "NZD",
                Amount = 10000,
            });
        }
    }
}
