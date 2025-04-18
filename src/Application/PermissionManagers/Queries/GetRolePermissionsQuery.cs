using Escrow.Api.Application.Common.Models.AssignPermissionDtos;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Interfaces;

public record GetRoleMenuPermissionsQuery : IRequest<PaginatedList<RoleMenuPermissionDto>>
{
    public int? UserId { get; init; }   // Nullable UserId for filtering
    public int? PageNumber { get; init; } = 1; // Nullable page number
    public int? PageSize { get; init; } = 10;  // Nullable page size
}


public class GetRoleMenuPermissionsQueryHandler : IRequestHandler<GetRoleMenuPermissionsQuery, PaginatedList<RoleMenuPermissionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRoleMenuPermissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<RoleMenuPermissionDto>> Handle(GetRoleMenuPermissionsQuery request, CancellationToken cancellationToken)
    {
        // Ensure nullable values are converted or defaulted to non-nullable types
        int pageNumber = request.PageNumber ?? 1; // Default to 1 if null
        int pageSize = request.PageSize ?? 10;    // Default to 10 if null

        // Fetch the RoleMenuPermissions data from the database (no grouping here)
        var query = _context.RoleMenuPermissions.AsQueryable();



        if (request.UserId.HasValue)
        {
            query = query.Where(r => r.UserId == request.UserId.Value);
        }




        // Fetch data with required filtering (add any filtering logic here if needed)
        var result = await query.ToListAsync(cancellationToken);

        // Group by UserId and MenuId in memory
        var queryResult = result
            .GroupBy(r => r.UserId) // Group by UserId
            .Select(g => new RoleMenuPermissionDto
            {
                UserId = g.Key ?? 0, // Handle nullable UserId, default to 0 if null
                MenuPermissions = g.GroupBy(r => r.MenuId) // Group by MenuId
                                   .Select(mg => new MenuPermissionDto
                                   {
                                       MenuId = mg.Key, // Handle nullable MenuId, default to 0 if null
                                       PermissionIds = mg.Select(r => r.PermissionId).ToList() // Handle nullable PermissionId, default to 0 if null
                                   }).ToList()
            })
            .OrderBy(r => r.UserId)
            .ToList();

        // Apply pagination to the result
        var paginatedData = queryResult.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        var totalCount = queryResult.Count();

        return new PaginatedList<RoleMenuPermissionDto>(paginatedData, totalCount, pageNumber, pageSize);
    }

}



























//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.Common.Models.AssignPermissionDtos;
//using Escrow.Api.Application.DTOs;
//using MediatR;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;

//namespace Escrow.Api.Application.PermissionManagers.Queries
//{
//    public record GetRolePermissionsQuery(int UserId, int? PageNumber = 1, int? PageSize = 10) : IRequest<Result<PaginatedList<RoleMenuPermissionDto>>>;

//    public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, Result<PaginatedList<RoleMenuPermissionDto>>>
//    {
//        private readonly IApplicationDbContext _context;

//        public GetRolePermissionsQueryHandler(IApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<Result<PaginatedList<RoleMenuPermissionDto>>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
//        {
//            try
//            {
//                // Ensure PageNumber and PageSize are set to default values if not provided
//                int pageNumber = request.PageNumber ?? 1; // Default to 1 if null
//                int pageSize = request.PageSize ?? 10; // Default to 10 if null

//                // Fetch the grouped permissions from the database
//                var groupedPermissions = await _context.RoleMenuPermissions
//                    .Where(rmp => rmp.UserId == request.UserId)
//                    .GroupBy(rmp => new { rmp.UserId, rmp.MenuId })
//                    .Select(g => new
//                    {
//                        UserId = g.Key.UserId,
//                        MenuId = g.Key.MenuId,
//                        PermissionIds = g.Select(x => x.PermissionId).Distinct().ToList()
//                    })
//                    .ToListAsync(cancellationToken);

//                // Group by UserId to create RoleMenuPermissionDto
//                var groupedByUser = groupedPermissions
//                    .GroupBy(x => x.UserId)
//                    .Select(g => new RoleMenuPermissionDto
//                    {
//                        UserId = g.Key,
//                        MenuPermissions = g.Select(x => new MenuPermissionDto
//                        {
//                            MenuId = x.MenuId,
//                            PermissionIds = x.PermissionIds
//                        }).ToList()
//                    })
//                    .AsQueryable();  // Convert to IQueryable for pagination

//                if (!groupedByUser.Any())
//                {
//                    return Result<PaginatedList<RoleMenuPermissionDto>>.Failure(StatusCodes.Status404NotFound, "No permissions found.");
//                }

//                // Use PaginatedList.CreateAsync for pagination
//                var paginatedResult = await PaginatedList<RoleMenuPermissionDto>.CreateAsync(groupedByUser, pageNumber, pageSize);

//                return Result<PaginatedList<RoleMenuPermissionDto>>.Success(StatusCodes.Status200OK, "Fetched successfully", paginatedResult);
//            }
//            catch (Exception ex)
//            {
//                return Result<PaginatedList<RoleMenuPermissionDto>>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
//            }
//        }




//        //public async Task<Result<PaginatedList<RoleMenuPermissionDto>>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
//        //{
//        //    try
//        //    {
//        //        int pageNumber = request.PageNumber;
//        //        int pageSize = request.PageSize;

//        //        // Fetch permissions and apply pagination
//        //        var query = _context.RoleMenuPermissions
//        //            .Where(rmp => rmp.UserId == request.UserId)
//        //            .GroupBy(rmp => rmp.MenuId)
//        //            .Select(g => new MenuPermissionDto
//        //            {
//        //                MenuId = g.Key,
//        //                PermissionIds = g.Select(x => x.PermissionId).Distinct().ToList()
//        //            });

//        //        var totalCount = await query.CountAsync(cancellationToken); // Get the total count for pagination
//        //        var items = await query
//        //            .Skip((pageNumber - 1) * pageSize)
//        //            .Take(pageSize)
//        //            .ToListAsync(cancellationToken);

//        //        if (!items.Any())
//        //        {
//        //            return Result<PaginatedList<RoleMenuPermissionDto>>.Failure(StatusCodes.Status404NotFound, "No permissions found.");
//        //        }

//        //        // Convert MenuPermissionDto to RoleMenuPermissionDto
//        //        var result = new PaginatedList<RoleMenuPermissionDto>(
//        //            items.Select(item => new RoleMenuPermissionDto
//        //            {
//        //                UserId = request.UserId,
//        //                MenuPermissions = new List<MenuPermissionDto> { item } // Wrapping MenuPermissionDto into RoleMenuPermissionDto
//        //            }).ToList(),
//        //            totalCount,
//        //            pageNumber,
//        //            pageSize
//        //        );

//        //        return Result<PaginatedList<RoleMenuPermissionDto>>.Success(StatusCodes.Status200OK, "Fetched successfully", result);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        return Result<PaginatedList<RoleMenuPermissionDto>>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
//        //    }
//        //}
//    }
//}






































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.Common.Models.AssignPermissionDtos;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.PermissionManagers.Queries;

//public record GetRolePermissionsQuery(int RoleId) : IRequest<Result<List<RoleMenuPermissionDto>>>;


//public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, Result<List<RoleMenuPermissionDto>>>
//{
//    private readonly IApplicationDbContext _context;

//    public GetRolePermissionsQueryHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<List<RoleMenuPermissionDto>>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            var result = await _context.RoleMenuPermissions
//                .Where(x => x.RoleId == request.RoleId)
//                .Select(x => new
//                {
//                    Menu = _context.Menus.Where(m => m.Id == x.MenuId).Select(m => m.Name).FirstOrDefault(),
//                    Permission = _context.Permissions.Where(p => p.Id == x.PermissionId).Select(p => p.Name).FirstOrDefault()
//                })
//                .GroupBy(x => x.Menu)
//                .Select(g => new RoleMenuPermissionDto
//                {
//                    Menu = g.Key,
//                    Permissions = g.Where(p => !string.IsNullOrWhiteSpace(p.Permission))
//                                   .Select(p => p.Permission!)
//                                   .Distinct()
//                                   .ToList()
//                })
//                .ToListAsync(cancellationToken);

//            return result.Any()
//                ? Result<List<RoleMenuPermissionDto>>.Success(StatusCodes.Status200OK, "Fetched successfully", result)
//                : Result<List<RoleMenuPermissionDto>>.Failure(StatusCodes.Status404NotFound, "No permissions found.");
//        }
//        catch (Exception ex)
//        {
//            var exp = ex;
//        }

//    }
//}


