using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.RoleMenuPermissions;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.PermissionManagers.Commands;

public class AssignPermissionsCommand : IRequest<Result<AssignPermissionsResultDto>>
{
    public int UserId { get; set; }
    public List<MenuPermissionDto> MenuPermissions { get; set; } = new();
}

public class MenuPermissionDto
{
    public int MenuId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class AssignPermissionsResultDto
{
    public int UserId { get; set; }
    public List<int> RemovedMenuIds { get; set; } = new();
    public List<AssignedMenuPermissionsDto> AssignedPermissions { get; set; } = new();
}

public class AssignedMenuPermissionsDto
{
    public int MenuId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class AssignPermissionsCommandHandler : IRequestHandler<AssignPermissionsCommand, Result<AssignPermissionsResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AssignPermissionsCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<AssignPermissionsResultDto>> Handle(AssignPermissionsCommand request, CancellationToken cancellationToken)
    {
        int currentUserId = _jwtService.GetUserId().ToInt(); // Optional use

        var menuIds = request.MenuPermissions.Select(mp => mp.MenuId).Distinct().ToList();

        // Remove existing permissions
        var existingPermissions = await _context.RoleMenuPermissions
            .Where(x => x.UserId == request.UserId && menuIds.Contains(x.MenuId))
            .ToListAsync(cancellationToken);

        var removedMenuIds = existingPermissions.Select(x => x.MenuId).Distinct().ToList();
        _context.RoleMenuPermissions.RemoveRange(existingPermissions);

        // Create new permissions
        var newPermissions = request.MenuPermissions
            .SelectMany(mp => mp.PermissionIds.Select(pid => new RoleMenuPermission
            {
                UserId = request.UserId,
                MenuId = mp.MenuId,
                PermissionId = pid
            }))
            .ToList();

        await _context.RoleMenuPermissions.AddRangeAsync(newPermissions, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Build response DTO
        var resultDto = new AssignPermissionsResultDto
        {
            UserId = request.UserId,
            RemovedMenuIds = removedMenuIds,
            AssignedPermissions = request.MenuPermissions
                .Select(mp => new AssignedMenuPermissionsDto
                {
                    MenuId = mp.MenuId,
                    PermissionIds = mp.PermissionIds
                }).ToList()
        };

        return Result<AssignPermissionsResultDto>.Success(StatusCodes.Status200OK, "Permissions assigned successfully", resultDto);
    }
}



































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Entities.RoleMenuPermissions;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.PermissionManagers.Commands;
//public class AssignPermissionsCommand : IRequest<Result<bool>>
//{
//    public int UserId { get; set; }
//    public int MenuId { get; set; }
//    public List<int> PermissionIds { get; set; } = new();
//}

//public class AssignPermissionsCommandHandler : IRequestHandler<AssignPermissionsCommand, Result<bool>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;

//    public AssignPermissionsCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//    {
//        _context = context;
//        _jwtService = jwtService;
//    }

//    public async Task<Result<bool>> Handle(AssignPermissionsCommand request, CancellationToken cancellationToken)
//    {
//        // Optional: you can use this if needed later
//        int userId = _jwtService.GetUserId().ToInt();

//        // Remove existing permissions for this role & menu
//        var existingPermissions = await _context.RoleMenuPermissions
//            .Where(x => x.UserId == request.UserId && x.MenuId == request.MenuId)
//            .ToListAsync(cancellationToken);

//        _context.RoleMenuPermissions.RemoveRange(existingPermissions);

//        // Add new permissions
//        var newPermissions = request.PermissionIds.Select(pid => new RoleMenuPermission
//        {
//            UserId = request.UserId,
//            MenuId = request.MenuId,
//            PermissionId = pid
//        }).ToList();

//        await _context.RoleMenuPermissions.AddRangeAsync(newPermissions, cancellationToken);
//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<bool>.Success(StatusCodes.Status200OK, "Permissions assigned successfully", true);
//    }
//}
