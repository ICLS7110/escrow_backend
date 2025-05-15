using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.AMLPanel;
public class AMLTransactionVerification : BaseAuditableEntity
{
    public string? TransactionId { get; set; } // Unique flagged transaction ID
    public string? AdminId { get; set; } // Admin who reviewed the transaction
    public AMLAdminAction AdminAction { get; set; } // Enum (Approve, Hold, Flag)
    public string? Remarks { get; set; } // Additional comments by admin
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow; // Review timestamp
}
