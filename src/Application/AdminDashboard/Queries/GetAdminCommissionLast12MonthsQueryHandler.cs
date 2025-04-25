using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.AdminDashboard.Queries;

public class GetAdminCommissionLast12MonthsQuery : IRequest<Result<List<MonthlyValueDto>>>
{
    public int? Year { get; set; }  // Optional
    public int? Month { get; set; } // Optional
}
public class GetAdminCommissionLast12MonthsQueryHandler : IRequestHandler<GetAdminCommissionLast12MonthsQuery, Result<List<MonthlyValueDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAdminCommissionLast12MonthsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MonthlyValueDto>>> Handle(GetAdminCommissionLast12MonthsQuery request, CancellationToken cancellationToken)
    {
        var result = new List<MonthlyValueDto>();

        try
        {
            var query = _context.ContractDetails
                .AsNoTracking()
                .Where(c => c.EscrowTax > 0); // optional sanity filter

            // Optional: filter by specific year/month
            if (request.Year.HasValue)
            {
                query = query.Where(c => c.Created.Year == request.Year.Value);
            }

            if (request.Month.HasValue)
            {
                query = query.Where(c => c.Created.Month == request.Month.Value);
            }

            // Group by Year + Month
            var grouped = await query
                .GroupBy(c => new
                {
                    c.Created.Year,
                    c.Created.Month
                })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    Value = g.Sum(c => (decimal?)c.EscrowTax ?? 0)
                })
                .ToListAsync(cancellationToken);

            if (request.Year.HasValue && request.Month.HasValue)
            {
                // Only one month
                var month = new DateTime(request.Year.Value, request.Month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                result.Add(new MonthlyValueDto
                {
                    Month = month.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    Value = grouped.FirstOrDefault()?.Value ?? 0
                });
            }
            else
            {
                // Default: Fill up to 12 months range from current date or specified year
                var now = DateTime.UtcNow;
                var startMonth = request.Year.HasValue
                    ? new DateTime(request.Year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    : new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);

                for (int i = 0; i < 12; i++)
                {
                    var month = startMonth.AddMonths(i);
                    var formatted = month.ToString("MMM yyyy", CultureInfo.InvariantCulture);
                    var existing = grouped.FirstOrDefault(g => g.Month.Month == month.Month && g.Month.Year == month.Year);

                    result.Add(new MonthlyValueDto
                    {
                        Month = formatted,
                        Value = existing?.Value ?? 0
                    });
                }
            }
        }
        catch (Exception ex)
        {
            return Result<List<MonthlyValueDto>>.Failure(
                StatusCodes.Status417ExpectationFailed,
                $"Failed to retrieve admin commission data: {ex.Message}"
            );
        }

        return Result<List<MonthlyValueDto>>.Success(
            StatusCodes.Status200OK,
            "Admin commission data retrieved.",
            result
        );
    }

}

//public async Task<Result<List<MonthlyValueDto>>> Handle(GetAdminCommissionLast12MonthsQuery request, CancellationToken cancellationToken)
//    {
//        var result = new List<MonthlyValueDto>();

//        try
//        {
//            var now = DateTime.UtcNow;
//            var fromDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);

//            var grouped = await _context.ContractDetails
//                .AsNoTracking()
//                .Where(c => c.Created >= fromDate)
//                .GroupBy(c => new
//                {
//                    Year = c.Created.Year,
//                    Month = c.Created.Month
//                })
//                .Select(g => new
//                {
//                    Month = new DateTime(g.Key.Year, g.Key.Month, 1, 0, 0, 0, DateTimeKind.Utc),
//                    Value = g.Sum(c => (decimal?)c.EscrowTax ?? 0)
//                })
//                .ToListAsync(cancellationToken);

//            for (int i = 0; i < 12; i++)
//            {
//                var month = fromDate.AddMonths(i);
//                var formattedMonth = month.ToString("MMM yyyy", CultureInfo.InvariantCulture);
//                var existing = grouped.FirstOrDefault(g => g.Month.Month == month.Month && g.Month.Year == month.Year);

//                result.Add(new MonthlyValueDto
//                {
//                    Month = formattedMonth,
//                    Value = existing?.Value ?? 0
//                });
//            }
//        }
//        catch (Exception ex)
//        {
//            return Result<List<MonthlyValueDto>>.Failure(
//                StatusCodes.Status417ExpectationFailed,
//                $"Failed to retrieve admin commission data: {ex.Message}"
//            );
//        }

//        return Result<List<MonthlyValueDto>>.Success(
//            StatusCodes.Status200OK,
//            "Admin commission data retrieved.",
//            result
//        );
//    }
