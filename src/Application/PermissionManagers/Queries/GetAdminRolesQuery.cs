using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.AssignPermissionDtos;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.PermissionManagers.Queries
{
    public class GetAdminRolesQuery : IRequest<Result<List<RoleDto>>> { }

    public class GetAdminRolesQueryHandler : IRequestHandler<GetAdminRolesQuery, Result<List<RoleDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetAdminRolesQueryHandler(IApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Result<List<RoleDto>>> Handle(GetAdminRolesQuery request, CancellationToken cancellationToken)
        {
            var roleNames = await _context.UserDetails
                .Where(x => x.Role != null)
                .Select(x => x.Role!.ToString())  // Extract the role name (enum value)
                .ToListAsync(cancellationToken);

            if (roleNames == null || !roleNames.Any())
            {
                return Result<List<RoleDto>>.Failure(StatusCodes.Status404NotFound, "No roles found.");
            }

            // Normalize and select unique role names (case-insensitive comparison)
            var uniqueRoleNames = roleNames
                .Select(role => role.ToLower()) // Convert role names to lowercase for case-insensitive comparison
                .Distinct()                    // Ensure uniqueness
                .ToList();

            var result = uniqueRoleNames
                .Select(name => new RoleDto
                {
                    Name = name // You can also apply title casing if needed, e.g., ToTitleCase
                })
                .ToList();

            return Result<List<RoleDto>>.Success(StatusCodes.Status200OK, "Roles fetched successfully", result);
        }
    }
}
