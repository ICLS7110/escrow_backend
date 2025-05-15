using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.UserPanel.Queries.GetUsers;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.Common.Models;
public class FlaggedTransactionDto
{
    public string? Id { get; set; }
    public string? TransactionId { get; set; }
    public string? UserId { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; } // Pending, Approved, Held, Flagged
    public string? Reason { get; set; }
    public string? CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }
    public virtual UserDetail? User { get; set; }
}
