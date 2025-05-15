using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.AMLPanel;
public class AMLNotification : BaseAuditableEntity
{

    public string? TransactionId { get; set; }

    public string? UserId { get; set; }

    public string? Message { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
