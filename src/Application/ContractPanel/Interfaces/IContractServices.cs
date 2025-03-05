using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.ContractPanel;

namespace Escrow.Api.Application.ContractPanel.Interfaces;
public interface IContractServices
{
    Task<ContractDetails?> FindContractAsync(int contractId);
}
