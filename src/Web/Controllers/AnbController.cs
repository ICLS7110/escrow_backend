using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.Payments;
using Escrow.Api.Application.Features.Bank.Queries;
using Escrow.Api.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnbController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAnbService _anbService;

    public AnbController(IMediator mediator,IAnbService anbService)
    {
        _mediator = mediator;
        _anbService = anbService;
    }

    [HttpGet("balance/{accountNumber}")]
    public async Task<IActionResult> GetBalance(string accountNumber)
    {
        var result = await _mediator.Send(new GetAccountBalanceQuery(accountNumber));
        return Ok(result);
    }

    [HttpPost("verify-account")]
    public async Task<IActionResult> VerifyAccount([FromBody] VerifyAccountRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Iban) ||
            string.IsNullOrWhiteSpace(request.NationalId) || string.IsNullOrWhiteSpace(request.DestinationBankBIC))
        {
            return BadRequest("All fields are required: iban, nationalId, and destinationBankBIC.");
        }

        var account = await _anbService.VerifyAccountAsync(request);
        return Ok(new { account });
    }


    [HttpGet("token")]
    public async Task<IActionResult> GetToken()
    {
        try
        {
            var token = await _anbService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Access token is null or empty." });

            return Ok(new { accessToken = token });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { message = "Error while calling ANB token API", detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
        }
    }


    [HttpPost("transfer")]
    public async Task<IActionResult> BankToBankTransfer([FromBody] PaymentRequest request)
    {
        var result = await _anbService.MakeBankToBankTransferAsync(request);
        return Ok(result);
    }

    [HttpGet("payment-status/{id}")]
    public async Task<IActionResult> GetPaymentStatus(string id)
    {
        var result = await _anbService.GetPaymentStatusAsync(id);
        return Ok(result);
    }

    [HttpGet("sanctions-check/{nationalId}")]
    public async Task<IActionResult> CheckSanctions(string nationalId)
    {
        var result = await _anbService.CheckSanctionsAsync(nationalId);
        return Ok(result);
    }




}

