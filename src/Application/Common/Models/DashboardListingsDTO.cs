using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Models.ContractDTOs;

namespace Escrow.Api.Application.Common.Models;
public class DashboardListingsDTO
{
    public List<ContractDTO>? RecentContracts { get; set; }
    public List<DisputeDTO>? RecentDisputes { get; set; }
}
