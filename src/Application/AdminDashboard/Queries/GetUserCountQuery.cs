
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Escrow.Api.Application.Common.Helpers;

namespace Escrow.Api.Application.AdminDashboard.Queries
{
    public class GetUserCountQuery : IRequest<Result<object>>
    {
    }

    public class GetUserCountQueryHandler : IRequestHandler<GetUserCountQuery, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetUserCountQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<object>> Handle(GetUserCountQuery request, CancellationToken cancellationToken)
        {
            // Check if HttpContext is available and then get the language
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;  // Use null-coalescing operator to fall back to English

            // Count active, non-deleted users with the role 'User'
            var count = await _context.UserDetails
                .AsNoTracking()
                .Where(u =>
                    u.Role == nameof(Roles.User) &&
                    u.IsActive == true &&
                    u.IsDeleted == false)
                .CountAsync(cancellationToken);

            return Result<object>.Success(
                StatusCodes.Status200OK,
                AppMessages.Get("UserCountRetrieved", language), // Use AppMessages.Get to fetch the localized message
                new { TotalUserCount = count }
            );
        }
    }
}




































//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Enums;
//using MediatR;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;

//namespace Escrow.Api.Application.AdminDashboard.Queries;

//public class GetUserCountQuery : IRequest<Result<object>>
//{
//}

//public class GetUserCountQueryHandler : IRequestHandler<GetUserCountQuery, Result<object>>
//{
//    private readonly IApplicationDbContext _context;

//    public GetUserCountQueryHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<object>> Handle(GetUserCountQuery request, CancellationToken cancellationToken)
//    {
//        var count = await _context.UserDetails
//            .AsNoTracking()
//            .Where(u =>
//                u.Role == nameof(Roles.User) &&
//                u.IsActive == true &&
//                u.IsDeleted == false)
//            .CountAsync(cancellationToken);

//        return Result<object>.Success(
//            StatusCodes.Status200OK,
//            AppMessages.UserCount,
//            new { TotalUserCount = count }
//        );
//    }
//}
