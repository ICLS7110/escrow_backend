using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.AssignPermissionDtos;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.PermissionManagers.Queries;

public class GetPermissionsQuery : IRequest<Result<List<PermissionDto>>> { }

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, Result<List<PermissionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetPermissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PermissionDto>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _context.Permissions
            .AsNoTracking()
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
            })
            .ToListAsync(cancellationToken);

        if (permissions == null || !permissions.Any())
        {
            return Result<List<PermissionDto>>.Failure(StatusCodes.Status404NotFound, "No permissions found.");
        }

        return Result<List<PermissionDto>>.Success(StatusCodes.Status200OK, "Permissions fetched successfully", permissions);
    }
}

