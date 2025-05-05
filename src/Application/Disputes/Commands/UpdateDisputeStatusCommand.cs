using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.Disputes.Commands;

public record UpdateDisputeStatusCommand(int DisputeId, DisputeStatus NewStatus) : IRequest<Result<string>>;

public class UpdateDisputeStatusCommandHandler : IRequestHandler<UpdateDisputeStatusCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public UpdateDisputeStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(UpdateDisputeStatusCommand request, CancellationToken cancellationToken)
    {
        var dispute = await _context.Disputes
            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

        if (dispute == null)
        {
            return Result<string>.Failure(404, "Dispute not found.");
        }

        if (dispute.Status == nameof(DisputeStatus.Resolved))
        {
            return Result<string>.Failure(400, "Dispute is already resolved and cannot be updated.");
        }

        dispute.Status = nameof(request.NewStatus);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(200, $"Dispute status updated to {request.NewStatus}.");
    }
}
