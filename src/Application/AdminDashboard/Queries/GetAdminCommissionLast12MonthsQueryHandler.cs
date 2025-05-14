using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Applcation.Common.Models;

namespace Escrow.Api.Application.AdminDashboard.Queries
{
    public class GetAdminCommissionLast12MonthsQuery : IRequest<Result<MonthlyValueDto>>
    {
        public TimeRangeType RangeType { get; set; } = TimeRangeType.Weekly;
    }

    public class GetAdminCommissionLast12MonthsQueryHandler : IRequestHandler<GetAdminCommissionLast12MonthsQuery, Result<MonthlyValueDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetAdminCommissionLast12MonthsQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<MonthlyValueDto>> Handle(GetAdminCommissionLast12MonthsQuery request, CancellationToken cancellationToken)
        {
            var result = new MonthlyValueDto();
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            try
            {
                var now = DateTime.UtcNow.Date;
                var currentYear = now.Year;

                var contractList = await _context.ContractDetails
                    .AsNoTracking()
                    .Where(c => c.EscrowTax > 0 && c.Created.Year == currentYear)
                    .ToListAsync(cancellationToken);

                switch (request.RangeType)
                {
                    case TimeRangeType.Weekly:
                        for (int i = 6; i >= 0; i--)
                        {
                            var day = now.AddDays(-i);
                            var total = contractList
                                .Where(c => c.Created.Date == day)
                                .Sum(c => c.EscrowTax ?? 0);

                            result.Labels.Add(day.ToString("dd MMM", CultureInfo.InvariantCulture));
                            result.Values.Add(total);
                        }
                        break;

                    case TimeRangeType.Quarterly:
                        for (int i = 0; i < 4; i++)
                        {
                            var referenceDate = now.AddMonths(-i * 3);
                            var start = new DateTime(referenceDate.Year, ((referenceDate.Month - 1) / 3) * 3 + 1, 1);
                            var end = start.AddMonths(3);

                            var total = contractList
                                .Where(c => c.Created >= start && c.Created < end)
                                .Sum(c => c.EscrowTax ?? 0);

                            var label = $"Q{((start.Month - 1) / 3 + 1)} {start:yyyy}";
                            result.Labels.Add(label);
                            result.Values.Add(total);
                        }
                        break;

                    case TimeRangeType.Monthly:
                    default:
                        for (int i = 0; i < 12; i++)
                        {
                            var month = new DateTime(currentYear, 1, 1).AddMonths(i);
                            var nextMonth = month.AddMonths(1);

                            var total = contractList
                                .Where(c => c.Created >= month && c.Created < nextMonth)
                                .Sum(c => c.EscrowTax ?? 0);

                            if (month > now)
                                total = 0;

                            result.Labels.Add(month.ToString("MMM yyyy", CultureInfo.InvariantCulture));
                            result.Values.Add(total);
                        }
                        break;
                }

                return Result<MonthlyValueDto>.Success(
                    StatusCodes.Status200OK,
                    AppMessages.Get("CommissionDataRetrieved", language),
                    result
                );
            }
            catch (Exception ex)
            {
                return Result<MonthlyValueDto>.Failure(
                    StatusCodes.Status417ExpectationFailed,
                    $"Failed to retrieve admin commission data: {ex.Message}"
                );
            }
        }
    }
}








































//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using MediatR;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using Escrow.Api.Applcation.Common.Models;
//using Escrow.Api.Application.Common.Constants;

//namespace Escrow.Api.Application.AdminDashboard.Queries
//{
//    public class GetAdminCommissionLast12MonthsQuery : IRequest<Result<MonthlyValueDto>>
//    {
//        public TimeRangeType RangeType { get; set; } = TimeRangeType.Weekly;
//    }

//    public class GetAdminCommissionLast12MonthsQueryHandler : IRequestHandler<GetAdminCommissionLast12MonthsQuery, Result<MonthlyValueDto>>
//    {
//        private readonly IApplicationDbContext _context;

//        public GetAdminCommissionLast12MonthsQueryHandler(IApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<Result<MonthlyValueDto>> Handle(GetAdminCommissionLast12MonthsQuery request, CancellationToken cancellationToken)
//        {
//            var result = new MonthlyValueDto();

//            try
//            {
//                var now = DateTime.UtcNow.Date;
//                var currentYear = now.Year;

//                var contractList = await _context.ContractDetails
//                    .AsNoTracking()
//                    .Where(c => c.EscrowTax > 0 && c.Created.Year == currentYear)
//                    .ToListAsync(cancellationToken);

//                switch (request.RangeType)
//                {
//                    case TimeRangeType.Weekly:
//                        for (int i = 6; i >= 0; i--)
//                        {
//                            var day = now.AddDays(-i);
//                            var total = contractList
//                                .Where(c => c.Created.Date == day)
//                                .Sum(c => c.EscrowTax ?? 0);

//                            result.Labels.Add(day.ToString("dd MMM", CultureInfo.InvariantCulture));
//                            result.Values.Add(total);
//                        }
//                        break;

//                    case TimeRangeType.Quarterly:
//                        for (int i = 0; i < 4; i++)
//                        {
//                            var referenceDate = now.AddMonths(-i * 3);
//                            var start = new DateTime(referenceDate.Year, ((referenceDate.Month - 1) / 3) * 3 + 1, 1);
//                            var end = start.AddMonths(3);

//                            var total = contractList
//                                .Where(c => c.Created >= start && c.Created < end)
//                                .Sum(c => c.EscrowTax ?? 0);

//                            var label = $"Q{((start.Month - 1) / 3 + 1)} {start:yyyy}";
//                            result.Labels.Add(label);
//                            result.Values.Add(total);
//                        }
//                        break;

//                    case TimeRangeType.Monthly:
//                    default:
//                        for (int i = 0; i < 12; i++)
//                        {
//                            var month = new DateTime(currentYear, 1, 1).AddMonths(i);
//                            var nextMonth = month.AddMonths(1);

//                            var total = contractList
//                                .Where(c => c.Created >= month && c.Created < nextMonth)
//                                .Sum(c => c.EscrowTax ?? 0);

//                            if (month > now)
//                                total = 0;

//                            result.Labels.Add(month.ToString("MMM yyyy", CultureInfo.InvariantCulture));
//                            result.Values.Add(total);
//                        }
//                        break;
//                }

//                return Result<MonthlyValueDto>.Success(
//                    StatusCodes.Status200OK,
//                   AppMessages.CommissionDataRetrieved,
//                    result
//                );
//            }
//            catch (Exception ex)
//            {
//                return Result<MonthlyValueDto>.Failure(
//                    StatusCodes.Status417ExpectationFailed,
//                    $"Failed to retrieve admin commission data: {ex.Message}"
//                );
//            }
//        }
//    }
//}
