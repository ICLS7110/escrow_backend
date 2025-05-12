using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;

namespace Escrow.Api.Application.Features.Bank.Queries;
public class GetAccountBalanceQuery : IRequest<string>
{
    public string AccountNumber { get; set; }

    public GetAccountBalanceQuery(string accountNumber)
    {
        AccountNumber = accountNumber;
    }
}

public class GetAccountBalanceHandler : IRequestHandler<GetAccountBalanceQuery, string>
{
    private readonly IAnbService _anbService;

    public GetAccountBalanceHandler(IAnbService anbService)
    {
        _anbService = anbService;
    }

    public async Task<string> Handle(GetAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        return await _anbService.GetAccountBalanceAsync(request.AccountNumber);
    }
}
