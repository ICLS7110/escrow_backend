using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.Notifications;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.Disputes.Commands;

//public record UpdateDisputeStatusCommand(int DisputeId, DisputeStatus NewStatus, string ReleaseTo, string ReleaseAmount, string BuyerNote, string SellerNote) : IRequest<Result<string>>;

public class UpdateDisputeStatusCommand : IRequest<Result<string>>
{
    public int DisputeId { get; set; }
    public DisputeStatus NewStatus { get; set; }
    public string? ReleaseTo { get; set; }
    public string? ReleaseAmount { get; set; }
    public string? BuyerNote { get; set; }
    public string? SellerNote { get; set; }
}



public class UpdateDisputeStatusCommandHandler : IRequestHandler<UpdateDisputeStatusCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IJwtService _jwtService;
    private readonly INotificationService _notificationService;

    public UpdateDisputeStatusCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        INotificationService notificationService,
        IJwtService jwtService)
    {
        _context = context;
        _emailService = emailService;
        _notificationService = notificationService;
        _jwtService = jwtService;
    }

    public async Task<Result<string>> Handle(UpdateDisputeStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.GetUserId;

        // Fetch the dispute
        var dispute = await _context.Disputes
            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

        if (dispute == null)
            return Result<string>.Failure(404, "Dispute not found.");

        if (dispute.Status == nameof(DisputeStatus.Resolved))
            return Result<string>.Failure(400, "Dispute is already resolved and cannot be updated.");

        dispute.Status = request.NewStatus.ToString();
        dispute.ReleaseAmount = request.ReleaseAmount;
        dispute.SellerNote = request.NewStatus.ToString();
        dispute.ReleaseTo = request.ReleaseTo;

        // Fetch the related contract
        var contract = await _context.ContractDetails
            .FirstOrDefaultAsync(c => c.Id == dispute.ContractId, cancellationToken);

        if (contract != null)
        {
            contract.Status = request.NewStatus.ToString();
        }

        // Notify Buyer and Seller
        await _notificationService.SendNotificationAsync(
            creatorId: Convert.ToInt32(userId),
            buyerId: contract?.BuyerDetailsId ?? 0,
            sellerId: contract?.SellerDetailsId ?? 0,
            contractId: contract?.Id ?? 0,
            role: nameof(Roles.User), // Or nameof(Roles.Buyer/Seller) if role distinction needed
            cancellationToken: cancellationToken
        );

        // Send emails to Buyer and Seller
        var userIds = new[] { contract?.BuyerDetailsId, contract?.SellerDetailsId }
            .Where(id => id.HasValue).Select(id => id).ToList();

        var users = await _context.UserDetails
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            if (!string.IsNullOrWhiteSpace(user.EmailAddress) && !string.IsNullOrWhiteSpace(user.FullName))
            {
                var subject = "Dispute Status Updated";
                var body = $"Dear {user.FullName},\n\nThe dispute for contract #{contract?.Id} has been updated to '{request.NewStatus}'.\n\nThank you.";

                await _emailService.SendEmailAsync(
                    user.EmailAddress,
                    subject,
                    user.FullName,
                    body
                );
            }
        }

        // Notify Admins
        var adminUsers = await _context.UserDetails
            .Where(u => u.Role == nameof(Roles.Admin))
            .ToListAsync(cancellationToken);

        foreach (var admin in adminUsers)
        {
            await _notificationService.SendNotificationAsync(
                creatorId: Convert.ToInt32(userId),
                buyerId: 0,
                sellerId: admin.Id,
                contractId: contract?.Id ?? 0,
                role: nameof(Roles.Admin),
                cancellationToken: cancellationToken
            );
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(200, $"Dispute status updated to {request.NewStatus}.");
    }
}













//public class UpdateDisputeStatusCommandHandler : IRequestHandler<UpdateDisputeStatusCommand, Result<string>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IEmailService _emailService;
//    private readonly INotificationService _notificationService;

//    public UpdateDisputeStatusCommandHandler(
//        IApplicationDbContext context,
//        IEmailService emailService,
//        INotificationService notificationService)
//    {
//        _context = context;
//        _emailService = emailService;
//        _notificationService = notificationService;
//    }

//    public async Task<Result<string>> Handle(UpdateDisputeStatusCommand request, CancellationToken cancellationToken)
//    {
//        var dispute = await _context.Disputes
//            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

//        if (dispute == null)
//        {
//            return Result<string>.Failure(404, "Dispute not found.");
//        }

//        if (dispute.Status == nameof(DisputeStatus.Resolved))
//        {
//            return Result<string>.Failure(400, "Dispute is already resolved and cannot be updated.");
//        }

//        // Update dispute status
//        dispute.Status = request.NewStatus.ToString();

//        // Update related contract status if dispute is resolved
//        var contract = await _context.ContractDetails
//            .FirstOrDefaultAsync(c => c.Id == dispute.ContractId, cancellationToken);

//        if (contract != null)
//        {
//            contract.Status = request.NewStatus.ToString(); // Set the contract status to the new status
//        }

//        // Fetch emails of buyer and seller
//        var userIds = new[] { contract?.BuyerDetailsId, contract?.SellerDetailsId }.Where(id => id != null).ToList();

//        var users = await _context.UserDetails
//            .Where(u => userIds.Contains(u.Id))
//            .ToListAsync(cancellationToken);

//        foreach (var user in users)
//        {
//            if (!string.IsNullOrWhiteSpace(user.EmailAddress) && !string.IsNullOrWhiteSpace(user.FullName))
//            {
//                var subject = "Dispute Status Updated";
//                var body = $"Dear {user.FullName},\n\nThe dispute for contract #{contract?.Id} has been updated to '{request.NewStatus}'.\n\nThank you.";

//                await _emailService.SendEmailAsync(
//                    user.EmailAddress,
//                    subject,
//                    user.FullName,
//                    body
//                );
//            }
//        }



//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<string>.Success(200, $"Dispute status updated to {request.NewStatus}.");
//    }

//    //public async Task<Result<string>> Handle(UpdateDisputeStatusCommand request, CancellationToken cancellationToken)
//    //{
//    //    var dispute = await _context.Disputes
//    //        .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

//    //    if (dispute == null)
//    //    {
//    //        return Result<string>.Failure(404, "Dispute not found.");
//    //    }

//    //    if (dispute.Status == nameof(DisputeStatus.Resolved))
//    //    {
//    //        return Result<string>.Failure(400, "Dispute is already resolved and cannot be updated.");
//    //    }

//    //    // Update dispute status
//    //    dispute.Status = request.NewStatus.ToString();

//    //    // Update related contract status if dispute is resolved

//    //    var contract = await _context.ContractDetails
//    //        .FirstOrDefaultAsync(c => c.Id == dispute.ContractId, cancellationToken);

//    //    if (contract != null)
//    //    {
//    //        contract.Status = nameof(request.NewStatus); // Or use a constant/enum if available
//    //    }
//    //    // Send email notification
//    //    await _emailService.SendEmailAsync();

//    //    await _context.SaveChangesAsync(cancellationToken);

//    //    return Result<string>.Success(200, $"Dispute status updated to {request.NewStatus}.");
//    //}



//    //public async Task<Result<string>> Handle(UpdateDisputeStatusCommand request, CancellationToken cancellationToken)
//    //{
//    //    var dispute = await _context.Disputes
//    //        .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

//    //    if (dispute == null)
//    //    {
//    //        return Result<string>.Failure(404, "Dispute not found.");
//    //    }

//    //    if (dispute.Status == nameof(DisputeStatus.Resolved))
//    //    {
//    //        return Result<string>.Failure(400, "Dispute is already resolved and cannot be updated.");
//    //    }

//    //    dispute.Status = nameof(request.NewStatus);

//    //    await _context.SaveChangesAsync(cancellationToken);

//    //    return Result<string>.Success(200, $"Dispute status updated to {request.NewStatus}.");
//    //}
//}
