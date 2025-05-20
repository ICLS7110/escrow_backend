using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.Payments;
public class RefundResponseData
{
    public long RefundTransactionId { get; set; }
    public string? RefundStatus { get; set; }
    public decimal RefundedAmount { get; set; }
    public string? RefundReason { get; set; }
    public DateTime RefundDate { get; set; }
}
