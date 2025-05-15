using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.AdminDashboard.Queries;

public class GetEscrowAmountQuery : IRequest<Result<object>>
{
}

public class GetEscrowAmountQueryHandler : IRequestHandler<GetEscrowAmountQuery, Result<object>>
{
    private readonly IApplicationDbContext _context;

    public GetEscrowAmountQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object>> Handle(GetEscrowAmountQuery request, CancellationToken cancellationToken)
    {
        var escrowStatuses = new List<string>
        {
            ContractStatus.Escrow.ToString(),
            ContractStatus.Accepted.ToString()
        };

        var amountInEscrow = await _context.ContractDetails
            .AsNoTracking()
            .Where(e => escrowStatuses.Contains(e.Status))
            .SumAsync(e => (decimal?)e.FeeAmount ?? 0m, cancellationToken);

        return Result<object>.Success(
            StatusCodes.Status200OK,
            "Escrow amount fetched successfully.",
            new { AmountInEscrow = amountInEscrow }
        );
    }
}
