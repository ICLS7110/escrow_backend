using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.RoleMenuPermissions;
public class Menu : BaseAuditableEntity
{
    public string? Name { get; set; } // e.g., "Users", "Contracts", "Reports"
}
