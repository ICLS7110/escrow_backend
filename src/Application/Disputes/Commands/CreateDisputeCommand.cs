using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.Disputes;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Disputes.Commands;
public class CreateDisputeCommand : IRequest<Result<int>>
{
    public int ContractId { get; set; }
    public string? DisputeRaisedBy { get; set; } // Buyer/Seller
    public string? DisputeReason { get; set; }
    public string? DisputeDescription { get; set; }
    public string? DisputeDoc { get; set; } // Comma-separated
}










public class CreateDisputeCommandHandler : IRequestHandler<CreateDisputeCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public CreateDisputeCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<int>> Handle(CreateDisputeCommand request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Result<int>.Failure(StatusCodes.Status401Unauthorized, "User is not authenticated.");

        var contract = await _context.ContractDetails.FindAsync(new object[] { request.ContractId }, cancellationToken);
        if (contract == null)
            return Result<int>.Failure(StatusCodes.Status404NotFound, "Contract not found.");

        // ✅ Contract must be in Escrow status
        if (contract.Status != nameof(ContractStatus.Escrow))
            return Result<int>.Failure(StatusCodes.Status400BadRequest, "Dispute can only be raised while contract is in 'Escrow'.");

        if (contract.EscrowStatusUpdatedAt == null)
            return Result<int>.Failure(StatusCodes.Status400BadRequest, "Escrow timestamp missing; cannot verify dispute window.");

        var hoursSinceEscrow = DateTime.UtcNow - contract.EscrowStatusUpdatedAt.Value;
        if (hoursSinceEscrow.TotalHours > 48)
            return Result<int>.Failure(StatusCodes.Status400BadRequest, "Dispute must be raised within 48 hours of entering 'Escrow'.");

        // ✅ Check for existing pending dispute by same user for the same contract
        var existingDispute = await _context.Disputes
            .FirstOrDefaultAsync(d =>
                d.ContractId == request.ContractId &&
                d.DisputeRaisedBy == userId &&
                d.Status == nameof(DisputeStatus.Pending),
                cancellationToken);

        if (existingDispute != null)
        {
            return Result<int>.Failure(StatusCodes.Status400BadRequest,
                "A pending dispute already exists for this contract raised by you.");
        }

        // ✅ Create new dispute
        var dispute = new Dispute
        {
            DisputeDateTime = DateTime.UtcNow,
            ContractId = request.ContractId,
            DisputeRaisedBy = userId,
            DisputeReason = request.DisputeReason,
            DisputeDescription = request.DisputeDescription,
            DisputeDoc = request.DisputeDoc,
            Status = nameof(DisputeStatus.Pending)
        };

        _context.Disputes.Add(dispute);

        // ✅ Update contract status
        contract.Status = nameof(ContractStatus.Dispute);
        _context.ContractDetails.Update(contract);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(StatusCodes.Status500InternalServerError, ex.Message);
        }

        return Result<int>.Success(StatusCodes.Status200OK, "Dispute created and contract status updated successfully.", dispute.Id);
    }
}











//public class CreateDisputeCommandHandler : IRequestHandler<CreateDisputeCommand, Result<int>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;

//    public CreateDisputeCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//    {
//        _context = context;
//        _jwtService = jwtService;
//    }

//    public async Task<Result<int>> Handle(CreateDisputeCommand request, CancellationToken cancellationToken)
//    {



//        var userId = _jwtService.GetUserId();

//        if (string.IsNullOrEmpty(userId))
//            return Result<int>.Failure(StatusCodes.Status401Unauthorized, "User is not authenticated.");

//        var contract = await _context.ContractDetails.FindAsync(new object[] { request.ContractId }, cancellationToken);
//        if (contract == null)
//            return Result<int>.Failure(StatusCodes.Status404NotFound, "Contract not found.");



//        var dispute = new Dispute
//        {
//            DisputeDateTime = DateTime.UtcNow,
//            ContractId = request.ContractId,
//            DisputeRaisedBy = userId,
//            DisputeReason = request.DisputeReason,
//            DisputeDescription = request.DisputeDescription,
//            DisputeDoc = request.DisputeDoc,
//            Status = nameof(DisputeStatus.Pending)
//        };

//        _context.Disputes.Add(dispute);

//        // Update Contract Status to "Dispute"
//        contract.Status = nameof(ContractStatus.Dispute);
//        _context.ContractDetails.Update(contract);
//        try
//        {
//            await _context.SaveChangesAsync(cancellationToken);
//        }
//        catch (Exception ex)
//        {

//            var xxp = ex;
//        }
//        return Result<int>.Success(StatusCodes.Status200OK, "Dispute created and contract status updated successfully.", dispute.Id);
//    }

//}

