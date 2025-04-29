using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Commissions.Queries;

public record GetCommissionRateQuery : IRequest<Result<CommissionDTO>>
{
}


public class GetCommissionRateQueryHandler : IRequestHandler<GetCommissionRateQuery, Result<CommissionDTO>>
{
    private readonly IApplicationDbContext _context;

    public GetCommissionRateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CommissionDTO>> Handle(GetCommissionRateQuery request, CancellationToken cancellationToken)
    {
        var commission = await _context.CommissionMasters.OrderByDescending(c => c.LastModified).FirstOrDefaultAsync(cancellationToken);

        if (commission == null)
            return Result<CommissionDTO>.Failure(StatusCodes.Status404NotFound, "No commission data found.");

        return Result<CommissionDTO>.Success(StatusCodes.Status200OK, "Success", new CommissionDTO
        {
            Id = commission.Id,
            CommissionRate = commission.CommissionRate,
            AppliedGlobally = commission.AppliedGlobally,
            TransactionType = commission.TransactionType,
            TaxRate = commission.TaxRate,
        });
    }
}
