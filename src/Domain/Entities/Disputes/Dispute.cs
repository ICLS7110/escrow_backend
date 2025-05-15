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
    public string? ReleaseTo { get; set; }
    public string? ReleaseAmount { get; set; }
    public string? BuyerNote { get; set; }
    public string? SellerNote { get; set; }
    public DateTime DisputeDateTime { get; set; } = DateTime.UtcNow;
}
