using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.AdminDashboard.Queries;

public class GetUserCountQuery : IRequest<Result<object>>
{
}

public class GetUserCountQueryHandler : IRequestHandler<GetUserCountQuery, Result<object>>
{
    private readonly IApplicationDbContext _context;

    public GetUserCountQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object>> Handle(GetUserCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _context.UserDetails
            .AsNoTracking()
            .Where(u =>
                u.Role == nameof(Roles.User) &&
                u.IsActive == true &&
                u.IsDeleted == false)
            .CountAsync(cancellationToken);

        return Result<object>.Success(
            StatusCodes.Status200OK,
            "User count fetched successfully.",
            new { TotalUserCount = count }
        );
    }
}
