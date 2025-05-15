using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.Common.Models;

namespace Escrow.Api.Application.AdminDashboard.Queries;
public record GetDashboardListingsQuery : IRequest<DashboardListingsDTO>;

public class GetDashboardListingsQueryHandler : IRequestHandler<GetDashboardListingsQuery, DashboardListingsDTO>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardListingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardListingsDTO> Handle(GetDashboardListingsQuery request, CancellationToken cancellationToken)
    {
        var recentContracts = await _context.ContractDetails
            .OrderByDescending(c => c.Created)
            .Take(10)
            .Select(c => new ContractDTO { Id = c.Id, ContractTitle = c.ContractTitle }) // Customize as needed
            .ToListAsync(cancellationToken);

        var recentDisputes = await _context.Disputes
            .OrderByDescending(d => d.Created)
            .Take(10)
            .Select(d => new DisputeDTO { Id = d.Id }) // Customize as needed
            .ToListAsync(cancellationToken);

        return new DashboardListingsDTO
        {
            RecentContracts = recentContracts,
            RecentDisputes = recentDisputes
        };
    }
}
