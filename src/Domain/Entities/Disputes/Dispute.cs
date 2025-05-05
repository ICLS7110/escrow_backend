using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.ContractPanel;

namespace Escrow.Api.Domain.Entities.Disputes;

public class Dispute : BaseAuditableEntity
{
    public int ContractId { get; set; }
    public string? DisputeRaisedBy { get; set; } 
    public string? DisputeReason { get; set; }
    public string? DisputeDescription { get; set; }
    public string? DisputeDoc { get; set; } 
    public string? Status { get; set; } 
    public DateTime DisputeDateTime { get; set; } = DateTime.UtcNow;
}
//public class Dispute : BaseAuditableEntity
//{
//    public DateTime DisputeDateTime { get; set; }
//    public int ContractId { get; set; }
//    public string? DisputeRaisedBy { get; set; } // Buyer/Seller
//    public DisputeStatus Status { get; set; }  // Enum
//    public decimal EscrowAmount { get; set; }
//    public decimal ContractAmount { get; set; }
//    public decimal FeesTaxes { get; set; }
//    public List<DisputeMessage> Messages { get; set; } = new();
//    public int? ArbitratorId { get; set; }  // Optional Third-Party Arbitrator
//    public string? AdminDecision { get; set; }
//    public decimal? ReleaseAmount { get; set; }
//    public string? ReleaseTo { get; set; }  // Buyer/Seller
//    public virtual ContractDetails? ContractDetails { get; set; }  // ✅ Fixed (Nullable)
//}
