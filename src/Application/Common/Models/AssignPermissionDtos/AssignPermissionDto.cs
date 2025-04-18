using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.AssignPermissionDtos;
public class AssignPermissionDto
{
    public int RoleId { get; set; }
    public int MenuId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}
