using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Domain.Enums;
using Escrow.Api.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using Escrow.Api.Domain.Entities.Transactions;
using System.Transactions;

namespace Escrow.Api.Application.ContractPanel.ContractCommands;

public class UpdateContractStatusCommand : IRequest<bool>
{
    public int ContractId { get; set; }
    public string BuyerPhoneNumber { get; set; } = string.Empty;
    public string SellerPhoneNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? StatusReason { get; set; }
}

public class UpdateContractStatusCommandHandler : IRequestHandler<UpdateContractStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    private readonly ILogger<UpdateContractStatusCommandHandler> _logger;
    private readonly INotificationService _notificationService;

    public UpdateContractStatusCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IJwtService jwtService,
        INotificationService notificationService, ILogger<UpdateContractStatusCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _jwtService = jwtService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateContractStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            int userId = _jwtService.GetUserId().ToInt();

            var buyer = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.BuyerPhoneNumber, cancellationToken);

            var seller = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.SellerPhoneNumber, cancellationToken);

            if (buyer == null || seller == null) return false;

            var contract = await _context.ContractDetails
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null) return false;

            // Mark milestones as completed if contract is completed
            if (request.Status.Equals(nameof(ContractStatus.Completed), StringComparison.OrdinalIgnoreCase) || request.Status.Equals(nameof(ContractStatus.Released), StringComparison.OrdinalIgnoreCase))
            {
                var milestones = await _context.MileStones
                    .Where(m => m.ContractId == request.ContractId && m.RecordState == 0)
                    .ToListAsync(cancellationToken);

                foreach (var milestone in milestones)
                {
                    milestone.Status = nameof(MilestoneStatus.Completed);
                    milestone.LastModified = DateTime.UtcNow;
                    milestone.LastModifiedBy = userId.ToString();
                }

                _context.MileStones.UpdateRange(milestones);

                var totalAmount = milestones.Sum(m => m.Amount); // assuming milestone has an Amount property

                var transaction = new Domain.Entities.Transactions.Transaction
                {
                    TransactionDateTime = DateTime.UtcNow,
                    TransactionAmount = Convert.ToDecimal(contract.FeeAmount),
                    TransactionType = request.Status, // or "Completed" depending on your logic
                    FromPayee = contract.BuyerDetailsId.ToString(), // or appropriate logic
                    ToRecipient = contract.SellerDetailsId.ToString(), // or appropriate logic
                    ContractId = request.ContractId,
                    Status = nameof(ContractStatus.Released),
                    Created = DateTime.UtcNow,
                    CreatedBy = userId.ToString()
                };

                await _context.Transactions.AddAsync(transaction, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);



            }

            // Update contract status
            contract.Status = request.Status == nameof(ContractStatus.Released) ? nameof(ContractStatus.Completed) : request.Status;
            contract.StatusReason = request.StatusReason;
            contract.LastModified = DateTime.UtcNow;
            contract.LastModifiedBy = userId.ToString();

            _context.ContractDetails.Update(contract);
            await _context.SaveChangesAsync(cancellationToken);

            // Send notifications to buyer and seller
            var participants = new List<UserDetail> { buyer, seller };

            foreach (var user in participants)
            {
                await _notificationService.SendNotificationAsync(
                    creatorId: userId,
                    buyerId: buyer.Id,
                    sellerId: seller.Id,
                    contractId: contract.Id,
                    role: nameof(Roles.User),
                    type: "Contract",
                    cancellationToken: cancellationToken
                );
            }

            // Notify Admins
            var adminUsers = await _context.UserDetails
                .Where(u => u.Role == nameof(Roles.Admin))
                .ToListAsync(cancellationToken);

            foreach (var admin in adminUsers)
            {
                await _notificationService.SendNotificationAsync(
                    creatorId: userId,
                    buyerId: buyer.Id,
                    sellerId: admin.Id,
                    contractId: contract.Id,
                    role: nameof(Roles.Admin),
                    type: "Contract",
                    cancellationToken: cancellationToken
                );
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating contract status.");
            return false;
        }

    }
}










































//using MediatR;
//using Escrow.Api.Application.Common.Interfaces;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Escrow.Api.Domain.Enums;

//namespace Escrow.Api.Application.ContractPanel.ContractCommands;

//public class UpdateContractStatusCommand : IRequest<bool>
//{
//    public int ContractId { get; set; }
//    public string BuyerPhoneNumber { get; set; } = string.Empty;
//    public string SellerPhoneNumber { get; set; } = string.Empty;
//    public string Status { get; set; } = string.Empty;
//    public string? StatusReason { get; set; }
//}

//public class UpdateContractStatusCommandHandler : IRequestHandler<UpdateContractStatusCommand, bool>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IMapper _mapper;
//    private readonly IJwtService _jwtService;

//    public UpdateContractStatusCommandHandler(IApplicationDbContext context, IMapper mapper, IJwtService jwtService)
//    {
//        _context = context;
//        _mapper = mapper;
//        _jwtService = jwtService;
//    }

//    public async Task<bool> Handle(UpdateContractStatusCommand request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            int userId = _jwtService.GetUserId().ToInt(); // Get the authenticated user's ID

//            var buyer = await _context.UserDetails
//                .FirstOrDefaultAsync(u => u.PhoneNumber == request.BuyerPhoneNumber, cancellationToken);

//            var seller = await _context.UserDetails
//                .FirstOrDefaultAsync(u => u.PhoneNumber == request.SellerPhoneNumber, cancellationToken);

//            if (buyer == null || seller == null) return false; // Return false if buyer or seller is not found

//            var contract = await _context.ContractDetails
//                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

//            if (contract == null) return false; // Return false if the contract is not found


//            if (request.Status.ToString().Equals(nameof(ContractStatus.Completed), StringComparison.OrdinalIgnoreCase))
//            {
//                var milestones = await _context.MileStones
//                    .Where(m => m.ContractId == request.ContractId && m.RecordState == 0)
//                    .ToListAsync(cancellationToken);

//                foreach (var milestone in milestones)
//                {
//                    milestone.Status = nameof(MilestoneStatus.Completed); // Use your actual enum/value
//                    milestone.LastModified = DateTime.UtcNow;
//                    milestone.LastModifiedBy = userId.ToString();
//                }

//                _context.MileStones.UpdateRange(milestones);
//                await _context.SaveChangesAsync(cancellationToken);
//            }

//            // Update contract status details
//            contract.Status = request.Status;
//            contract.StatusReason = request.StatusReason;
//            contract.LastModified = DateTime.UtcNow;
//            contract.LastModifiedBy = contract.LastModifiedBy; // Store the user ID who modified the contract

//            _context.ContractDetails.Update(contract);
//            await _context.SaveChangesAsync();
//        }
//        catch (Exception ex)
//        {

//            var exp = ex;
//        }
//        return true;
//    }
//}
