using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Models.ContractDTOs;
public class CustomerContractDetailsDto
{
    public List<ContractDetailsDTO> ActiveContracts { get; set; } = new();
    public List<ContractDetailsDTO> HistoricalContracts { get; set; } = new();
    public int ActiveContractsTotal { get; set; }
    public int HistoricalContractsTotal { get; set; }
}
