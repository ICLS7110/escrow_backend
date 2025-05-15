
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Application.Disputes.Commands;

public record EscrowDecisionCommand(int DisputeId, decimal ReleaseAmount, string ReleaseTo, string AdminDecision)
    : IRequest<Result<string>>;

public class EscrowDecisionCommandHandler : IRequestHandler<EscrowDecisionCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EscrowDecisionCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<string>> Handle(EscrowDecisionCommand request, CancellationToken cancellationToken)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var dispute = await _context.Disputes
            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

        if (dispute == null)
        {
            return Result<string>.Failure(404, AppMessages.Get("DisputeNotFound", language));
        }

        //if (dispute.Status != DisputeStatus.InReview)
        //{
        //    return Result<string>.Failure(400, AppMessages.Get("EscrowDecisionOnlyInReview", language));
        //}

        //if (request.ReleaseAmount > dispute.EscrowAmount)
        //{
        //    return Result<string>.Failure(400, AppMessages.Get("ReleaseAmountExceedsEscrow", language));
        //}

        if (request.ReleaseTo.ToLower() != "buyer" && request.ReleaseTo.ToLower() != "seller")
        {
            return Result<string>.Failure(400, AppMessages.Get("InvalidReleaseRecipient", language));
        }

        // Update dispute record with escrow decision
        //dispute.ReleaseAmount = request.ReleaseAmount;
        //dispute.ReleaseTo = request.ReleaseTo;
        //dispute.AdminDecision = request.AdminDecision;
        dispute.Status = nameof(DisputeStatus.Resolved);  // Mark dispute as resolved

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(200,
            string.Format(AppMessages.Get("EscrowDecisionApplied", language), request.ReleaseAmount, request.ReleaseTo));
    }
}




































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Enums;

//namespace Escrow.Api.Application.Disputes.Commands;

//public record EscrowDecisionCommand(int DisputeId, decimal ReleaseAmount, string ReleaseTo, string AdminDecision)
//    : IRequest<Result<string>>;

//public class EscrowDecisionCommandHandler : IRequestHandler<EscrowDecisionCommand, Result<string>>
//{
//    private readonly IApplicationDbContext _context;

//    public EscrowDecisionCommandHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<string>> Handle(EscrowDecisionCommand request, CancellationToken cancellationToken)
//    {
//        var dispute = await _context.Disputes
//            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

//        if (dispute == null)
//        {
//            return Result<string>.Failure(404, "Dispute not found.");
//        }

//        //if (dispute.Status != DisputeStatus.InReview)
//        //{
//        //    return Result<string>.Failure(400, "Escrow decision can only be made for disputes in 'InReview' status.");
//        //}

//        //if (request.ReleaseAmount > dispute.EscrowAmount)
//        //{
//        //    return Result<string>.Failure(400, "Release amount cannot be greater than the escrow amount.");
//        //}

//        if (request.ReleaseTo.ToLower() != "buyer" && request.ReleaseTo.ToLower() != "seller")
//        {
//            return Result<string>.Failure(400, "Invalid release recipient. Must be 'Buyer' or 'Seller'.");
//        }

//        // Update dispute record with escrow decision
//        //dispute.ReleaseAmount = request.ReleaseAmount;
//        //dispute.ReleaseTo = request.ReleaseTo;
//        //dispute.AdminDecision = request.AdminDecision;
//        dispute.Status = nameof(DisputeStatus.Resolved);  // Mark dispute as resolved

//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<string>.Success(200, $"Escrow decision applied successfully. {request.ReleaseAmount} released to {request.ReleaseTo}.");
//    }
//}
