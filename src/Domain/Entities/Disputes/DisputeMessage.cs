using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Domain.Entities.Disputes;
public class DisputeMessage : BaseAuditableEntity
{
    public int DisputeId { get; set; }
    public string? Sender { get; set; } // Buyer/Seller/Admin
    public string? Message { get; set; }
    public DateTime SentAt { get; set; }
}
