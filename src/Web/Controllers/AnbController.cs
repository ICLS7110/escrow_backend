using Escrow.Api.Application.Features.Bank.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnbController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnbController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("balance/{accountNumber}")]
    public async Task<IActionResult> GetBalance(string accountNumber)
    {
        var result = await _mediator.Send(new GetAccountBalanceQuery(accountNumber));
        return Ok(result);
    }
}

