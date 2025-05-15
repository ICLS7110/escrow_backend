using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Application.Disputes.Commands;

public class UpdateStatusCommand : IRequest<Result<string>>
{
    public int DisputeId { get; set; }
    public DisputeStatus NewStatus { get; set; }
    public string? ReleaseTo { get; set; }
    public string? ReleaseAmount { get; set; }
    public string? BuyerNote { get; set; }
    public string? SellerNote { get; set; }
}

public class UpdateStatusCommandHandler : IRequestHandler<UpdateStatusCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IJwtService _jwtService;
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateStatusCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        INotificationService notificationService,
        IJwtService jwtService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _emailService = emailService;
        _notificationService = notificationService;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<string>> Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;
        List<string> errorMessages = new();

        var userId = _jwtService.GetUserId().ToInt();

        var dispute = await _context.Disputes
            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

        if (dispute == null)
            return Result<string>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("DisputeNotFound", language));

        if (dispute.Status == nameof(DisputeStatus.Resolved))
            return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("DisputeAlreadyResolved", language));

        var contract = await _context.ContractDetails
            .FirstOrDefaultAsync(c => c.Id == dispute.ContractId, cancellationToken);

        if (contract == null)
            return Result<string>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("AssociatedContractNotFound", language));

        // Business rules: validate required fields for resolution
        if (request.NewStatus == DisputeStatus.Resolved)
        {
            if (string.IsNullOrWhiteSpace(request.ReleaseTo))
                return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("ReleaseToRequired", language));

            if (string.IsNullOrWhiteSpace(request.ReleaseAmount))
                return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("ReleaseAmountRequired", language));
        }

        // Update dispute details
        dispute.Status = request.NewStatus.ToString();
        dispute.ReleaseAmount = request.ReleaseAmount;
        dispute.SellerNote = request.SellerNote;
        dispute.BuyerNote = request.BuyerNote;
        dispute.ReleaseTo = request.ReleaseTo;

        contract.Status = request.NewStatus.ToString();

        // Notify Buyer and Seller via email
        var userIds = new[] { contract.BuyerDetailsId, contract.SellerDetailsId }
            .Where(id => id.HasValue)
            .Select(id => id)
            .ToList();

        var users = await _context.UserDetails
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(user.EmailAddress) && !string.IsNullOrWhiteSpace(user.FullName))
                {
                    var subject = AppMessages.Get("DisputeStatusUpdatedSubject", language);
                    var body = string.Format(AppMessages.Get("DisputeStatusUpdatedBody", language), user.FullName, contract.Id, request.NewStatus);

                    await _emailService.SendEmailAsync(user.EmailAddress, subject, user.FullName, body);
                }

                await _notificationService.SendNotificationAsync(
                    creatorId: userId,
                    buyerId: 0,
                    sellerId: user.Id,
                    contractId: contract.Id,
                    role: nameof(Roles.User),
                    type: "Dispute",
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                errorMessages.Add(string.Format(AppMessages.Get("NotifyUserFailed", language), user.FullName, ex.Message));
            }
        }

        // Notify Admins
        var adminUsers = await _context.UserDetails
            .Where(u => u.Role == nameof(Roles.Admin))
            .ToListAsync(cancellationToken);

        foreach (var admin in adminUsers)
        {
            await _notificationService.SendNotificationAsync(
                creatorId: userId,
                buyerId: 0,
                sellerId: admin.Id,
                contractId: contract.Id,
                role: nameof(Roles.Admin),
                type: "Dispute",
                cancellationToken: cancellationToken
            );
        }

        await _context.SaveChangesAsync(cancellationToken);

        var successMessage = string.Format(AppMessages.Get("DisputeStatusUpdatedSuccess", language), request.NewStatus);

        if (errorMessages.Any())
        {
            var fullMessage = successMessage + "\n" + AppMessages.Get("WithNotificationErrors", language) + "\n" + string.Join("\n", errorMessages);
            return Result<string>.Success(207, fullMessage); // 207: Multi-Status
        }

        return Result<string>.Success(StatusCodes.Status200OK, successMessage);
    }
}














































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Resources;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Enums;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.Disputes.Commands;

//public class UpdateStatusCommand : IRequest<Result<string>>
//{
//    public int DisputeId { get; set; }
//    public DisputeStatus NewStatus { get; set; }
//    public string? ReleaseTo { get; set; }
//    public string? ReleaseAmount { get; set; }
//    public string? BuyerNote { get; set; }
//    public string? SellerNote { get; set; }
//}

//public class UpdateStatusCommandHandler : IRequestHandler<UpdateStatusCommand, Result<string>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IEmailService _emailService;
//    private readonly IJwtService _jwtService;
//    private readonly INotificationService _notificationService;

//    public UpdateStatusCommandHandler(
//        IApplicationDbContext context,
//        IEmailService emailService,
//        INotificationService notificationService,
//        IJwtService jwtService)
//    {
//        _context = context;
//        _emailService = emailService;
//        _notificationService = notificationService;
//        _jwtService = jwtService;
//    }

//    public async Task<Result<string>> Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
//    {
//        List<string> errorMessages = new();
//        var userId = _jwtService.GetUserId().ToInt();

//        var dispute = await _context.Disputes
//            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

//        if (dispute == null)
//            return Result<string>.Failure(StatusCodes.Status404NotFound, "Dispute not found.");

//        if (dispute.Status == nameof(DisputeStatus.Resolved))
//            return Result<string>.Failure(StatusCodes.Status400BadRequest, "Dispute has already been resolved.");

//        var contract = await _context.ContractDetails
//            .FirstOrDefaultAsync(c => c.Id == dispute.ContractId, cancellationToken);

//        if (contract == null)
//            return Result<string>.Failure(StatusCodes.Status404NotFound, "Associated contract not found.");

//        // Business rules: validate required fields for resolution
//        if (request.NewStatus == DisputeStatus.Resolved)
//        {
//            if (string.IsNullOrWhiteSpace(request.ReleaseTo))
//                return Result<string>.Failure(StatusCodes.Status400BadRequest, "ReleaseTo is required when resolving a dispute.");

//            if (string.IsNullOrWhiteSpace(request.ReleaseAmount))
//                return Result<string>.Failure(StatusCodes.Status400BadRequest, "ReleaseAmount is required when resolving a dispute.");
//        }

//        // Update dispute details
//        dispute.Status = request.NewStatus.ToString();
//        dispute.ReleaseAmount = request.ReleaseAmount;
//        dispute.SellerNote = request.SellerNote;
//        dispute.BuyerNote = request.BuyerNote;
//        dispute.ReleaseTo = request.ReleaseTo;

//        contract.Status = request.NewStatus.ToString();

//        // Notify Buyer and Seller via email
//        var userIds = new[] { contract.BuyerDetailsId, contract.SellerDetailsId }.Where(id => id.HasValue).Select(id => id).ToList();

//        var users = await _context.UserDetails
//            .Where(u => userIds.Contains(u.Id))
//            .ToListAsync(cancellationToken);

//        foreach (var user in users)
//        {
//            try
//            {
//                if (!string.IsNullOrWhiteSpace(user.EmailAddress) && !string.IsNullOrWhiteSpace(user.FullName))
//                {
//                    var subject = "Dispute Status Updated";
//                    var body = $"Dear {user.FullName},\n\nThe dispute for contract #{contract.Id} has been updated to '{request.NewStatus}'.\n\nThank you.";

//                    await _emailService.SendEmailAsync(user.EmailAddress, subject, user.FullName, body);
//                }

//                await _notificationService.SendNotificationAsync(
//                    creatorId: userId,
//                    buyerId: 0,
//                    sellerId: user.Id,
//                    contractId: contract.Id,
//                    role: nameof(Roles.User),
//                    type: "Dispute",
//                    cancellationToken: cancellationToken
//                );
//            }
//            catch (Exception ex)
//            {
//                errorMessages.Add($"Failed to notify {user.FullName}: {ex.Message}");
//            }

//        }

//        // Notify Admins
//        var adminUsers = await _context.UserDetails
//            .Where(u => u.Role == nameof(Roles.Admin))
//            .ToListAsync(cancellationToken);

//        foreach (var admin in adminUsers)
//        {
//            await _notificationService.SendNotificationAsync(
//                creatorId: userId,
//                buyerId: 0,
//                sellerId: admin.Id,
//                contractId: contract.Id,
//                role: nameof(Roles.Admin),
//                type: "Dispute",
//                cancellationToken: cancellationToken
//            );
//        }

//        await _context.SaveChangesAsync(cancellationToken);

//        var successMessage = $"Dispute status successfully updated to '{request.NewStatus}'.";
//        if (errorMessages.Any())
//        {
//            var fullMessage = successMessage + "\nHowever, the following issues occurred:\n" + string.Join("\n", errorMessages);
//            return Result<string>.Success(207, fullMessage); // 207: Multi-Status
//        }

//        return Result<string>.Success(StatusCodes.Status200OK, successMessage);

//        //return Result<string>.Success( $"Dispute status successfully updated to '{request.NewStatus}'.");
//    }
//}


//public async Task<Result<string>> Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
//{
//    var userId = _jwtService.GetUserId().ToInt();

//    // Fetch the dispute
//    var dispute = await _context.Disputes
//        .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

//    if (dispute == null)
//        return Result<string>.Failure(404, "Dispute not found.");

//    if (dispute.Status == nameof(DisputeStatus.Resolved))
//        return Result<string>.Failure(400, "Dispute is already resolved and cannot be updated.");

//    dispute.Status = request.NewStatus.ToString();
//    dispute.ReleaseAmount = request.ReleaseAmount;
//    dispute.SellerNote = request.SellerNote;
//    dispute.BuyerNote = request.BuyerNote;
//    dispute.ReleaseTo = request.ReleaseTo;

//    // Fetch the related contract
//    var contract = await _context.ContractDetails
//        .FirstOrDefaultAsync(c => c.Id == dispute.ContractId, cancellationToken);

//    if (contract != null)
//    {
//        contract.Status = request.NewStatus.ToString();
//    }

//    //// Notify Buyer and Seller
//    //await _notificationService.SendNotificationAsync(
//    //    creatorId: Convert.ToInt32(userId),
//    //    buyerId: contract?.BuyerDetailsId ?? 0,
//    //    sellerId: contract?.SellerDetailsId ?? 0,
//    //    contractId: contract?.Id ?? 0,
//    //    role: nameof(Roles.User), // Or nameof(Roles.Buyer/Seller) if role distinction needed
//    //    cancellationToken: cancellationToken
//    //);

//    // Send emails to Buyer and Seller
//    var userIds = new[] { contract?.BuyerDetailsId, contract?.SellerDetailsId }
//        .Where(id => id.HasValue).Select(id => id).ToList();

//    var users = await _context.UserDetails
//        .Where(u => userIds.Contains(u.Id))
//        .ToListAsync(cancellationToken);

//    foreach (var user in users)
//    {
//        if (!string.IsNullOrWhiteSpace(user.EmailAddress) && !string.IsNullOrWhiteSpace(user.FullName))
//        {
//            var subject = "Dispute Status Updated";
//            var body = $"Dear {user.FullName},\n\nThe dispute for contract #{contract?.Id} has been updated to '{request.NewStatus}'.\n\nThank you.";

//            await _emailService.SendEmailAsync(
//                user.EmailAddress,
//                subject,
//                user.FullName,
//                body
//            );

//            await _notificationService.SendNotificationAsync(
//                creatorId: Convert.ToInt32(userId),
//                buyerId: 0,
//                sellerId: user.Id,
//                contractId: contract?.Id ?? 0,
//                role: nameof(Roles.Admin),
//                cancellationToken: cancellationToken
//            );
//        }
//    }


//            // Notify Admins
//    var adminUsers = await _context.UserDetails
//        .Where(u => u.Role == nameof(Roles.Admin))
//        .ToListAsync(cancellationToken);

//    foreach (var admin in adminUsers)
//    {
//        await _notificationService.SendNotificationAsync(
//            creatorId: Convert.ToInt32(userId),
//            buyerId: 0,
//            sellerId: admin.Id,
//            contractId: contract?.Id ?? 0,
//            role: nameof(Roles.Admin),
//            cancellationToken: cancellationToken
//        );
//    }

//    await _context.SaveChangesAsync(cancellationToken);

//    return Result<string>.Success(200, $"Dispute status updated to {request.NewStatus}.");
//}






















//public async Task<Result<string>> Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
//   {
//       var userId = _jwtService.GetUserId().ToInt();

//       // Fetch the dispute
//       var dispute = await _context.Disputes
//           .FirstOrDefaultAsync(d => d.Id == request.DisputeId, cancellationToken);

//       if (dispute == null)
//           return Result<string>.Failure(404, "Dispute not found.");

//       if (dispute.Status == nameof(DisputeStatus.Resolved))
//           return Result<string>.Failure(400, "Dispute is already resolved and cannot be updated.");

//       dispute.Status = request.NewStatus.ToString();
//       dispute.ReleaseAmount = request.ReleaseAmount;
//       dispute.SellerNote = request.SellerNote;
//       dispute.BuyerNote = request.BuyerNote;
//       dispute.ReleaseTo = request.ReleaseTo;

//       // Fetch the related contract
//       var contract = await _context.ContractDetails
//           .FirstOrDefaultAsync(c => c.Id == dispute.ContractId, cancellationToken);

//       if (contract != null)
//       {
//           contract.Status = request.NewStatus.ToString();
//       }

//       //// Notify Buyer and Seller
//       //await _notificationService.SendNotificationAsync(
//       //    creatorId: Convert.ToInt32(userId),
//       //    buyerId: contract?.BuyerDetailsId ?? 0,
//       //    sellerId: contract?.SellerDetailsId ?? 0,
//       //    contractId: contract?.Id ?? 0,
//       //    role: nameof(Roles.User), // Or nameof(Roles.Buyer/Seller) if role distinction needed
//       //    cancellationToken: cancellationToken
//       //);

//       // Send emails to Buyer and Seller
//       var userIds = new[] { contract?.BuyerDetailsId, contract?.SellerDetailsId }
//           .Where(id => id.HasValue).Select(id => id).ToList();

//       var users = await _context.UserDetails
//           .Where(u => userIds.Contains(u.Id))
//           .ToListAsync(cancellationToken);

//       foreach (var user in users)
//       {
//           if (!string.IsNullOrWhiteSpace(user.EmailAddress) && !string.IsNullOrWhiteSpace(user.FullName))
//           {
//               var subject = "Dispute Status Updated";
//               var body = $"Dear {user.FullName},\n\nThe dispute for contract #{contract?.Id} has been updated to '{request.NewStatus}'.\n\nThank you.";

//               await _emailService.SendEmailAsync(
//                   user.EmailAddress,
//                   subject,
//                   user.FullName,
//                   body
//               );

//               await _notificationService.SendNotificationAsync(
//                   creatorId: Convert.ToInt32(userId),
//                   buyerId: 0,
//                   sellerId: user.Id,
//                   contractId: contract?.Id ?? 0,
//                   role: nameof(Roles.Admin),
//                   cancellationToken: cancellationToken
//               );
//           }
//       }


//       // Notify Admins
//       var adminUsers = await _context.UserDetails
//           .Where(u => u.Role == nameof(Roles.Admin))
//           .ToListAsync(cancellationToken);

//       foreach (var admin in adminUsers)
//       {
//           await _notificationService.SendNotificationAsync(
//               creatorId: Convert.ToInt32(userId),
//               buyerId: 0,
//               sellerId: admin.Id,
//               contractId: contract?.Id ?? 0,
//               role: nameof(Roles.Admin),
//               cancellationToken: cancellationToken
//           );
//       }

//       await _context.SaveChangesAsync(cancellationToken);

//       return Result<string>.Success(200, $"Dispute status updated to {request.NewStatus}.");
//   }
