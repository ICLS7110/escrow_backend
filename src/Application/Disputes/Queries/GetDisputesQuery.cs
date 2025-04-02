using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Domain.Common;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.Disputes.Queries;
public record GetDisputesQuery : IRequest<PaginatedList<DisputeDTO>>
{
    public DisputeStatus? Status { get; init; }  // Optional filter
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetDisputesQueryHandler : IRequestHandler<GetDisputesQuery, PaginatedList<DisputeDTO>>
{
    private readonly IApplicationDbContext _context;

    public GetDisputesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<DisputeDTO>> Handle(GetDisputesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Disputes
            .Include(d => d.Messages)  // Ensure messages are loaded
            .Include(d => d.ContractDetails) // Ensure contract details are loaded
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(d => d.Status == request.Status);
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        // Fetch data from DB first, then process null values in memory
        var disputes = await query
            .OrderByDescending(d => d.DisputeDateTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // ✅ Process null values after fetching data from DB
        var disputeDTOs = disputes.Select(d => new DisputeDTO
        {
            Id = d.Id,
            DisputeDateTime = d.DisputeDateTime,
            RaisedBy = d.DisputeRaisedBy ?? "Unknown", // ✅ Default if null
            Status = d.Status.ToString(),
            EscrowAmount = d.EscrowAmount,
            ContractAmount = d.ContractAmount,
            FeesTaxes = d.FeesTaxes,
            Messages = d.Messages?.Select(m => m.Message ?? "No message").ToList() ?? new List<string>(), // ✅ Handled safely
            ContractDetails = d.ContractDetails != null ? d.ContractDetails.ContractTitle : "N/A" // ✅ Avoid null reference
        }).ToList();

        return new PaginatedList<DisputeDTO>(disputeDTOs, totalRecords, request.PageNumber, request.PageSize);
    }


}
