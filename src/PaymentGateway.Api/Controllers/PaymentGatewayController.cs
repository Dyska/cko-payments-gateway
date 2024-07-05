using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Domain.Models;
using PaymentGateway.Api.Domain.Services;
using PaymentGateway.Api.Mappers;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentGatewayController : ControllerBase
{
    private readonly ILogger<PaymentGatewayController> _logger;
    private readonly IPaymentService _paymentService;

    public PaymentGatewayController(IPaymentService paymentService, ILogger<PaymentGatewayController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet("{id}", Name = "GetPaymentDetails")]
    [ProducesResponseType<GetPaymentDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetPaymentDetails([FromRoute] Guid id)
    {
        return Ok(new GetPaymentDetailsResponse
        {
            Id = id,
            Status = "Authorized",
            CardDetails = new CardResponse
            {
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
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest body)
    {
        //Don't forget idempotency token!

        Payment payment;
        try
        {
            payment = body.ToPayment();
        }
        catch (ArgumentException ex)
        {
            //TODO: Where should declined status live?
            return BadRequest(new { Status = "Declined", ex.Message });
        }

        //We've now got a payment object, ready to be processed
        Payment? updatedPayment = await _paymentService.ProcessPayment(payment);

        //TODO: Map back to response model

        return Created(
            "", //TODO: Construct URI for response
            new ProcessPaymentResponse
            {
                Id = Guid.NewGuid(),
                Status = "Authorised",
                CardDetails = new CardResponse
                {
                    CardNumberFinalFourDigits = "1234",
                    ExpiryMonth = "01",
                    ExpiryYear = "2025",
                },
                ISOCurrencyCode = "NZD",
                Amount = 10000, //TODO: Decimal?
            });
    }
}