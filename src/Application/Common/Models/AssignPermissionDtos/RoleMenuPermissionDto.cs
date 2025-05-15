using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.AssignPermissionDtos;
public class RoleMenuPermissionDto
{
    public int UserId { get; set; }
    public List<MenuPermissionDto> MenuPermissions { get; set; } = new List<MenuPermissionDto>();  // Initialize with empty list
}

public class MenuPermissionDto
{
    public int MenuId { get; set; }
    public List<int> PermissionIds { get; set; } = new List<int>();  // Initialize with empty list
}

