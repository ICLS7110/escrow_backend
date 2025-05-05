using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Escrow.Api.Application.Commissions.Queries
{
    public record GetCommissionRateQuery : IRequest<Result<List<CommissionDTO>>>
    {
        public int? Id { get; init; }
    }

    public class GetCommissionRateQueryHandler : IRequestHandler<GetCommissionRateQuery, Result<List<CommissionDTO>>>
    {
        private readonly IApplicationDbContext _context;

        public GetCommissionRateQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CommissionDTO>>> Handle(GetCommissionRateQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CommissionMasters.AsNoTracking().AsQueryable();

            if (request.Id.HasValue)
            {
                query = query.Where(c => c.Id == request.Id.Value);
            }

            var commissionList = await query
                .OrderByDescending(c => c.LastModified)
                .Select(c => new CommissionDTO
                {
                    Id = c.Id,
                    CommissionRate = c.CommissionRate,
                    AppliedGlobally = c.AppliedGlobally,
                    TransactionType = c.TransactionType,
                    TaxRate = c.TaxRate,
                    MinAmount = c.MinAmount,
                })
                .ToListAsync(cancellationToken);

            if (!commissionList.Any())
                return Result<List<CommissionDTO>>.Failure(StatusCodes.Status404NotFound, "No commission data found.");

            return Result<List<CommissionDTO>>.Success(StatusCodes.Status200OK, "Success", commissionList);
        }
    }
}
