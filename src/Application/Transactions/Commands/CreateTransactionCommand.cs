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

    public CreateTransactionCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<object>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
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

        if (!int.TryParse(_jwtService.GetUserId(), out int userId))
        {
            return Result<object>.Failure(StatusCodes.Status401Unauthorized, "Invalid user ID.");
        }

        int recipientId;
        if (userId == buyer.Id)
        {
            recipientId = seller.Id;
        }
        else if (userId == seller.Id)
        {
            recipientId = buyer.Id;
        }
        else
        {
            return Result<object>.Failure(StatusCodes.Status401Unauthorized, "Unauthorized transaction.");
        }

        var entity = new Domain.Entities.Transactions.Transaction
        {
            TransactionAmount = request.Amount,
            TransactionType = request.Type,
            ContractId = request.ContractId,
            Created = DateTime.UtcNow,
            FromPayee = userId.ToString(),
            ToRecipient = recipientId.ToString(),
            CreatedBy = userId.ToString()
        };

        _context.Transactions.Add(entity);

        // Update ContractDetails status to 'Accepted'
        contract.Status = nameof(ContractStatus.Escrow); // Assuming Status is a string, change as needed
        _context.ContractDetails.Update(contract);

        // Get all milestones for the contract
        var milestones = await _context.MileStones
            .Where(m => m.ContractId == request.ContractId)
            .ToListAsync(cancellationToken);

        // If milestones exist, update their statuses to 'Escrow'
        if (milestones.Any())
        {
            foreach (var milestone in milestones)
            {
                milestone.Status = nameof(MilestoneStatus.Escrow);
                milestone.LastModified = DateTime.UtcNow;
                milestone.LastModifiedBy = userId.ToString();
            }

            _context.MileStones.UpdateRange(milestones);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<object>.Success(StatusCodes.Status200OK, "Transaction created successfully.", new { TransactionId = entity.Id });
    }
}
