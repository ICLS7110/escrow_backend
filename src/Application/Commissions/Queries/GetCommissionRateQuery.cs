
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
using Escrow.Api.Application.Common.Constants;
using Microsoft.Extensions.Localization;

namespace Escrow.Api.Application.Commissions.Queries
{
    public record GetCommissionRateQuery : IRequest<Result<List<CommissionDTO>>>
    {
        public int? Id { get; init; }
    }

    public class GetCommissionRateQueryHandler : IRequestHandler<GetCommissionRateQuery, Result<List<CommissionDTO>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetCommissionRateQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<List<CommissionDTO>>> Handle(GetCommissionRateQuery request, CancellationToken cancellationToken)
        {
            // Get the current language from HttpContext
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

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
            {
                return Result<List<CommissionDTO>>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("CommissionDataNotFound", language));
            }

            return Result<List<CommissionDTO>>.Success(StatusCodes.Status200OK, AppMessages.Get("Success", language), commissionList);
        }
    }
}








































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using MediatR;
//using Escrow.Api.Application.Common.Constants;

//namespace Escrow.Api.Application.Commissions.Queries
//{
//    public record GetCommissionRateQuery : IRequest<Result<List<CommissionDTO>>>
//    {
//        public int? Id { get; init; }
//    }

//    public class GetCommissionRateQueryHandler : IRequestHandler<GetCommissionRateQuery, Result<List<CommissionDTO>>>
//    {
//        private readonly IApplicationDbContext _context;

//        public GetCommissionRateQueryHandler(IApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<Result<List<CommissionDTO>>> Handle(GetCommissionRateQuery request, CancellationToken cancellationToken)
//        {
//            var query = _context.CommissionMasters.AsNoTracking().AsQueryable();

//            if (request.Id.HasValue)
//            {
//                query = query.Where(c => c.Id == request.Id.Value);
//            }

//            var commissionList = await query
//                .OrderByDescending(c => c.LastModified)
//                .Select(c => new CommissionDTO
//                {
//                    Id = c.Id,
//                    CommissionRate = c.CommissionRate,
//                    AppliedGlobally = c.AppliedGlobally,
//                    TransactionType = c.TransactionType,
//                    TaxRate = c.TaxRate,
//                    MinAmount = c.MinAmount,
//                })
//                .ToListAsync(cancellationToken);

//            if (!commissionList.Any())
//                return Result<List<CommissionDTO>>.Failure(StatusCodes.Status404NotFound, AppMessages.CommissionDataNotFound);

//            return Result<List<CommissionDTO>>.Success(StatusCodes.Status200OK, AppMessages.Success, commissionList);
//        }
//    }
//}
