using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Domain.Entities.TeamMembers;
public class TeamMember : BaseAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string? RoleType { get; set; }
    public string? ContractId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }

}
