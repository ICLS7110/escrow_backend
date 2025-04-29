using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;

namespace Escrow.Api.Application.Disputes.Queries;

public record GetDisputeByIdQuery(int DisputeId) : IRequest<Result<DisputeDTO>>;

public class GetDisputeByIdQueryHandler : IRequestHandler<GetDisputeByIdQuery, Result<DisputeDTO>>
{
    private readonly IApplicationDbContext _context;

    public GetDisputeByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DisputeDTO>> Handle(GetDisputeByIdQuery request, CancellationToken cancellationToken)
    {
        var dispute = await _context.Disputes
            .Include(d => d.Messages)  // Include related messages
            .Include(d => d.ContractDetails) // Include contract details
            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

        if (dispute == null)
        {
            return Result<DisputeDTO>.Failure(404, "Dispute not found.");
        }

        // ✅ Map entity to DTO
        var disputeDTO = new DisputeDTO
        {
            Id = dispute.Id,
            DisputeDateTime = dispute.DisputeDateTime,
            RaisedBy = dispute.DisputeRaisedBy ?? "Unknown",
            Status = dispute.Status.ToString(),
            EscrowAmount = dispute.EscrowAmount,
            ContractAmount = dispute.ContractAmount,
            FeesTaxes = dispute.FeesTaxes,
            Messages = dispute.Messages?.Select(m => m.Message ?? "No message").ToList() ?? new List<string>(),
            ContractDetails = dispute.ContractDetails?.ContractTitle ?? "N/A",
            ReleaseAmount = dispute.ReleaseAmount ?? 0,  // Defaults to 0 if null
            ReleaseTo = dispute.ReleaseTo ?? "N/A"  // Defaults to "N/A" if null
        };


        return Result<DisputeDTO>.Success(200, "Dispute retrieved successfully.", disputeDTO);
    }
}
