using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.AdminDashboard.Queries;
public record GetDashboardCountsQuery : IRequest<DashboardCountsDTO>;

public class GetDashboardCountsQueryHandler : IRequestHandler<GetDashboardCountsQuery, DashboardCountsDTO>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardCountsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardCountsDTO> Handle(GetDashboardCountsQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await _context.UserDetails.CountAsync(u => u.IsDeleted == false && u.Role == nameof(Roles.User), cancellationToken);
        var totalSubAdmins = await _context.UserDetails.CountAsync(u => u.Role != null && u.Role.ToLower() == "sub-admin", cancellationToken);
        //var totalSubAdmins = await _context.UserDetails.CountAsync(u => u.Role != null &&u.Role.Replace("-", "").ToLower() == "subadmin", cancellationToken);


        var totalContracts = await _context.ContractDetails.CountAsync(cancellationToken);
        var ongoingContracts = await _context.ContractDetails.CountAsync(c =>
            c.Status != nameof(ContractStatus.Completed) &&
            c.Status != nameof(ContractStatus.Cancelled) &&
            c.Status != nameof(ContractStatus.Draft) &&
            c.Status != nameof(ContractStatus.Expired), cancellationToken);

        var escrowedAmount = await _context.ContractDetails.Where(c => c.Status != nameof(ContractStatus.Escrow))
            .SumAsync(c => (decimal?)c.FeeAmount ?? 0, cancellationToken);

        var escrowRevenue = await _context.ContractDetails
            .SumAsync(c => (decimal?)c.EscrowTax ?? 0, cancellationToken);

        var totalDisputeCount = await _context.Disputes.CountAsync(cancellationToken);

        return new DashboardCountsDTO
        {
            TotalUsers = totalUsers,
            TotalSubAdmins = totalSubAdmins,
            TotalContracts = totalContracts,
            OngoingContracts = ongoingContracts,
            EscrowedAmount = escrowedAmount,
            EscrowRevenue = escrowRevenue,
            TotalDisputeCount = totalDisputeCount
        };
    }
}

