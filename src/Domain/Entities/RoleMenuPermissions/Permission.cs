using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.RoleMenuPermissions;
public class Permission : BaseAuditableEntity
{
    public string? Name { get; set; } // Read, Write, View, Delete, etc.
}
