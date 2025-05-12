using Escrow.Api.Application.Common.Interfaces;
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
    public async Task<IActionResult> VerifyAccount([FromBody] string accountNumber)
    {
        var isValid = await _anbService.VerifyAccountAsync(accountNumber);
        return Ok(new { isValid });
    }
}

