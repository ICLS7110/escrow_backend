using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.Transactions.Commands;

public record CreateTransactionCommand : IRequest<Result<object>>
{
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public int ContractId { get; set; }
}

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public CreateTransactionCommandHandler(IApplicationDbContext context, IJwtService jwtService, IEmailService emailService, INotificationService notificationService)
    {
        _context = context;
        _jwtService = jwtService;
        _emailService = emailService;
        _notificationService = notificationService;
    }
    public async Task<Result<object>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        List<string> errorMessages = new();

        var userId = _jwtService.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Result<object>.Failure(StatusCodes.Status401Unauthorized, "User is not authenticated.");
        }

        var contract = await _context.ContractDetails.FindAsync(request.ContractId);
        if (contract == null)
        {
            return Result<object>.Failure(StatusCodes.Status404NotFound, "Contract not found.");
        }

        var buyer = await _context.UserDetails.FirstOrDefaultAsync(x => x.PhoneNumber == contract.BuyerMobile, cancellationToken);
        var seller = await _context.UserDetails.FirstOrDefaultAsync(x => x.PhoneNumber == contract.SellerMobile, cancellationToken);

        if (buyer == null || seller == null)
        {
            return Result<object>.Failure(StatusCodes.Status404NotFound, "Buyer or Seller not found.");
        }

        int currentUserId = Convert.ToInt32(userId);
        int recipientId;

        if (currentUserId == buyer.Id)
        {
            recipientId = seller.Id;
        }
        else if (currentUserId == seller.Id)
        {
            recipientId = buyer.Id;
        }
        else
        {
            return Result<object>.Failure(StatusCodes.Status401Unauthorized, "Unauthorized transaction.");
        }

        var transaction = new Domain.Entities.Transactions.Transaction
        {
            TransactionAmount = request.Amount,
            TransactionType = request.Type,
            ContractId = request.ContractId,
            Created = DateTime.UtcNow,
            FromPayee = userId,
            ToRecipient = recipientId.ToString(),
            CreatedBy = userId
        };

        _context.Transactions.Add(transaction);

        // Update contract and milestones status
        contract.Status = nameof(ContractStatus.Escrow);
        contract.EscrowStatusUpdatedAt = DateTime.UtcNow;
        _context.ContractDetails.Update(contract);

        var milestones = await _context.MileStones
            .Where(m => m.ContractId == request.ContractId)
            .ToListAsync(cancellationToken);

        foreach (var milestone in milestones)
        {
            milestone.Status = nameof(MilestoneStatus.Escrow);
            milestone.LastModified = DateTime.UtcNow;
            milestone.LastModifiedBy = userId;
        }

        if (milestones.Any())
        {
            _context.MileStones.UpdateRange(milestones);
        }

        // Notify Buyer and Seller
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
                    var subject = "Transaction Created";
                    var body = $"Dear {user.FullName},\n\nA new transaction has been created for your contract (ID: {contract.Id}).";

                    await _emailService.SendEmailAsync(user.EmailAddress, subject, user.FullName, body);
                }

                await _notificationService.SendNotificationAsync(
                    creatorId: currentUserId,
                    buyerId: 0,
                    sellerId: user.Id,
                    contractId: contract.Id,
                    role: nameof(Roles.User),
                    type: "Transaction",
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Failed to notify {user.FullName}: {ex.Message}");
            }
        }

        // Notify Admins
        var adminUsers = await _context.UserDetails
            .Where(u => u.Role == nameof(Roles.Admin))
            .ToListAsync(cancellationToken);

        foreach (var admin in adminUsers)
        {
            await _notificationService.SendNotificationAsync(
                creatorId: currentUserId,
                buyerId: 0,
                sellerId: admin.Id,
                contractId: contract.Id,
                role: nameof(Roles.Admin),
                type: "Transaction",
                cancellationToken: cancellationToken
            );
        }

        await _context.SaveChangesAsync(cancellationToken);

        var successMessage = "Transaction created successfully.";
        if (errorMessages.Any())
        {
            var fullMessage = successMessage + "\nHowever, the following issues occurred:\n" + string.Join("\n", errorMessages);
            return Result<object>.Success(StatusCodes.Status207MultiStatus, fullMessage);
        }

        return Result<object>.Success(StatusCodes.Status200OK, successMessage, new { TransactionId = transaction.Id });
    }
}
