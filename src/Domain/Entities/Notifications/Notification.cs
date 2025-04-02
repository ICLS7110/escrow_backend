using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.Notifications;
public class Notification : BaseAuditableEntity
{
    public int FromID { get; set; }
    public int ToID { get; set; }
    public int ContractId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
