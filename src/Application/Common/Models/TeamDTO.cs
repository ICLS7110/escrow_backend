using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.UserPanel.Queries.GetUsers;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.Common.Models;
public class TeamDTO
{
    public string TeamId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? RoleType { get; set; }
    public List<string>? ContractId { get; set; }

    public bool? IsActive { get; set; }
    public DateTimeOffset? Created { get; set; }
    public virtual UserDetailDto? User { get; set; }
    public virtual List<ContractDTO>? Contracts { get; set; }
}
