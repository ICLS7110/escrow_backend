using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Domain.Entities.AMLPanel;
public class AMLFlaggedTransaction : BaseAuditableEntity
{
    public string? TransactionId { get; set; } // Unique transaction identifier
    //[ForeignKey("UserId")]
    public string? UserId { get; set; } // Buyer/Seller ID
    public decimal Amount { get; set; } // Transaction amount
    public string Currency { get; set; } = "USD"; // Transaction currency (default USD)
    public string Status { get; set; } = "Pending"; // Enum for AML status
    public string? RiskReason { get; set; } // Reason for flagging (e.g., High Amount, Frequency)
    public DateTime FlaggedAt { get; set; } = DateTime.UtcNow; // Timestamp when flagged
    public string? AdminReviewedBy { get; set; } // Admin ID who reviewed the transaction
    public DateTime? ReviewedAt { get; set; } // When admin reviewed id

    // ✅ Added Verification Fields
    public bool IsVerified { get; set; } = false;  // Whether transaction is verified
    public DateTime? VerifiedAt { get; set; }  // When it was verified
    //public virtual UserDetail? User { get; set; }

}

