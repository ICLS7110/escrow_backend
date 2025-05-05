using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Disputes.Queries;

public record GetDisputeByIdQuery(int DisputeId) : IRequest<Result<DisputeDTO>>;

public class GetDisputeByIdQueryHandler : IRequestHandler<GetDisputeByIdQuery, Result<DisputeDTO>>
{
    private readonly IApplicationDbContext _context;

    private readonly IJwtService _jwtService;

    public GetDisputeByIdQueryHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<DisputeDTO>> Handle(GetDisputeByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Result<DisputeDTO>.Failure(StatusCodes.Status401Unauthorized, "User is not authenticated.");

        var dispute = await _context.Disputes
            .FirstOrDefaultAsync(d => d.Id == request.DisputeId && d.DisputeRaisedBy == userId, cancellationToken);

        if (dispute == null)
            return Result<DisputeDTO>.Failure(StatusCodes.Status404NotFound, "Dispute not found or access denied.");

        // Fetch contract separately
        var contract = await _context.ContractDetails
            .FirstOrDefaultAsync(c => c.Id == dispute.ContractId, cancellationToken);

        // Fetch messages separately
        var messages = await _context.DisputeMessages
            .Where(m => m.DisputeId == dispute.Id)
            .Select(m => m.Message ?? "No message")
            .ToListAsync(cancellationToken);

        var disputeDTO = new DisputeDTO
        {
            Id = dispute.Id,
            DisputeDateTime = dispute.DisputeDateTime,
            RaisedBy = dispute.DisputeRaisedBy ?? "Unknown",
            Status = dispute.Status ?? "N/A",
            EscrowAmount = contract?.EscrowTax ?? 0,
            ContractAmount = contract?.FeeAmount ?? 0,
            FeesTaxes = contract?.TaxAmount ?? 0,
            Messages = messages,
            DisputeDoc = dispute.DisputeDoc ?? "N/A",
            ContractDetails = contract?.Id.ToString() ?? "N/A",
        };

        return Result<DisputeDTO>.Success(StatusCodes.Status200OK, "Dispute retrieved successfully.", disputeDTO);
    }


}
