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
    public async Task<IActionResult> GetPaymentDetailsAsync([FromRoute] Guid id)
    {
        Payment? payment = await _paymentService.FetchPayment(id);
        GetPaymentDetailsResponse? response = payment.ToGetPaymentDetailsResponse();

        return response != null ? Ok(response) : NotFound();
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
            //TODO: Where should Rejected status live?
            return BadRequest(new { Status = "Rejected", ex.Message });
        }

        Payment submittedPayment = await _paymentService.ProcessPayment(payment);

        ProcessPaymentResponse response = submittedPayment.ToProcessPaymentResponse();
        return Created("", //TODO: Construct URI for response
            response
            );
    }
}