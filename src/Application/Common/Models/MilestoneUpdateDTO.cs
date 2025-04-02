using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
public class MilestoneUpdateDTO
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string MilestoneTitle { get; set; } = string.Empty;
    public string MilestoneDescription { get; set; } = string.Empty;
    public DateTimeOffset DueDate { get; set; }
    public decimal Amount { get; set; }
}
