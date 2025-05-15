using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models;
public class DashboardCountsDTO
{
    public int TotalUsers { get; set; }
    public int TotalContracts { get; set; }
    public int TotalSubAdmins { get; set; }
    public int OngoingContracts { get; set; }
    public decimal EscrowedAmount { get; set; }
    public decimal EscrowRevenue { get; set; }
    public int TotalDisputeCount { get; set; }
}
