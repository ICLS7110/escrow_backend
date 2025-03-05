using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.ContractPanel.Interfaces;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Infrastructure.Contracts;
public class ContractServices : IContractServices
{
    private readonly IApplicationDbContext _context;
    
    public ContractServices(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ContractDetails?> FindContractAsync(int contractId)
    {
        var contract = await _context.ContractDetails.Where(cont => cont.Id == contractId).FirstOrDefaultAsync();
        return contract;
    }
}
