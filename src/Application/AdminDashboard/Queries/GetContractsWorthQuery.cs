using System;
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

public class GetContractsWorthQuery : IRequest<Result<object>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetContractsWorthQueryHandler : IRequestHandler<GetContractsWorthQuery, Result<object>>
{
    private readonly IApplicationDbContext _context;

    public GetContractsWorthQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object>> Handle(GetContractsWorthQuery request, CancellationToken cancellationToken)
    {
        var validStatuses = new[]
        {
        ContractStatus.Accepted.ToString(),
        ContractStatus.Completed.ToString(),
        ContractStatus.Escrow.ToString()
    };

        var query = _context.ContractDetails
            .AsNoTracking()
            .Where(c => validStatuses.Contains(c.Status));

        // Optional date filters
        if (request.StartDate.HasValue)
        {
            query = query.Where(c => c.Created.Date >= request.StartDate.Value.Date);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(c => c.Created.Date <= request.EndDate.Value.Date);
        }

        var totalWorth = await query
            .SumAsync(c => (decimal?)c.FeeAmount ?? 0m, cancellationToken);

        return Result<object>.Success(
            StatusCodes.Status200OK,
            "Contracts worth fetched successfully.",
            new { TotalWorth = totalWorth }
        );
    }

}
