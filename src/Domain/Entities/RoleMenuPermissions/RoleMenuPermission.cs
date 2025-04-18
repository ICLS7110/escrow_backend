using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.RoleMenuPermissions;
public class RoleMenuPermission : BaseAuditableEntity
{
    public int? UserId { get; set; }
    public int MenuId { get; set; }
    public int PermissionId { get; set; }
}
