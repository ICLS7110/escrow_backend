using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;

namespace Escrow.Api.Application.Disputes.Commands;
public record AssignArbitratorCommand(int DisputeId, int ArbitratorId) : IRequest<Result<string>>;

public class AssignArbitratorCommandHandler : IRequestHandler<AssignArbitratorCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public AssignArbitratorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(AssignArbitratorCommand request, CancellationToken cancellationToken)
    {
        var dispute = await _context.Disputes
            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

        if (dispute == null)
        {
            return Result<string>.Failure(404, "Dispute not found.");
        }

        //if (dispute.ArbitratorId.HasValue)
        //{
        //    return Result<string>.Failure(400, "An arbitrator has already been assigned to this dispute.");
        //}

        //dispute.ArbitratorId = request.ArbitratorId;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(200, "Arbitrator assigned successfully.");
    }
}
