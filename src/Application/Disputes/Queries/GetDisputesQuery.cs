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
            var query = _context.Disputes.AsQueryable();

            if (!string.IsNullOrEmpty(nameof(request.Status)))
            {
                query = query.Where(d => d.Status == nameof(request.Status));
            }

            var totalRecords = await query.CountAsync(cancellationToken);

            var disputes = await query
                .OrderByDescending(d => d.DisputeDateTime)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var contractIds = disputes.Select(d => d.ContractId).Distinct().ToList();
            var contracts = await _context.ContractDetails
                .Where(c => contractIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.ContractTitle, cancellationToken);

            var disputeDTOs = disputes.Select(d => new DisputeDTO
            {
                Id = d.Id,
                DisputeDateTime = d.DisputeDateTime,
                RaisedBy = d.DisputeRaisedBy ?? "Unknown",
                DisputeDoc = d.DisputeDoc ?? "Unknown",
                Status = d.Status ?? "N/A",
                ContractDetails = contracts.TryGetValue(d.ContractId, out var title) ? title : "N/A"
            }).ToList();

            return new PaginatedList<DisputeDTO>(disputeDTOs, totalRecords, request.PageNumber, request.PageSize);
        }



}
