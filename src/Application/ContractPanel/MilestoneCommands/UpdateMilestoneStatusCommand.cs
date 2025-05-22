
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.ContractPanel.MilestoneCommands
{
    public class UpdateMilestoneStatusCommand : IRequest<Result<object>>
    {
        public int MilestoneId { get; set; }
    }

    public class UpdateMilestoneStatusCommandHandler : IRequestHandler<UpdateMilestoneStatusCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateMilestoneStatusCommandHandler(IApplicationDbContext context, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<object>> Handle(UpdateMilestoneStatusCommand request, CancellationToken cancellationToken)
        {
            // Retrieve the current language from the HTTP context
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            var milestone = await _context.MileStones
                .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

            if (milestone == null)
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("MilestoneNotFound", language));

            var userId = _jwtService.GetUserId();

            // Fetch the related contract
            var contract = await _context.ContractDetails
            .FirstOrDefaultAsync(c => c.Id == milestone.ContractId, cancellationToken);

            if (contract == null)
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("RelatedContractNotFound", language));

            if (milestone.Status != nameof(MilestoneStatus.Escrow))
            {
                // Retrieve the commission master to calculate the commission and tax
                var commission = await _context.CommissionMasters
                    .Where(c => c.AppliedGlobally == true)
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (commission == null)
                    return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("CommissionInformationNotFound", language));

                // Extract and calculate fee amount, escrow tax, and tax amount
                decimal feeAmount = milestone.Amount;
                decimal escrowTax = commission.CommissionRate;
                decimal taxRate = commission.TaxRate;

                decimal escrowAmount = (feeAmount * escrowTax) / 100;
                decimal taxAmount = (escrowAmount * taxRate) / 100;
                decimal totalAmount = feeAmount + taxAmount + escrowAmount;

                // Decide who pays the fee
                decimal buyerPayableAmount = 0;
                decimal sellerPayableAmount = 0;

                if (contract.FeesPaidBy.ToLower() == "buyer")
                {
                    buyerPayableAmount = feeAmount + escrowAmount;
                    sellerPayableAmount = feeAmount - taxAmount;
                }
                else if (contract.FeesPaidBy.ToLower() == "seller")
                {
                    sellerPayableAmount = feeAmount - taxAmount - escrowAmount;
                    buyerPayableAmount = feeAmount;
                }
                else if (contract.FeesPaidBy.ToLower() == "50" || contract.FeesPaidBy.ToLower() == "halfpayment")
                {
                    buyerPayableAmount = feeAmount + (escrowAmount / 2);
                    sellerPayableAmount = feeAmount - taxAmount - (escrowAmount / 2);
                }
                else
                {
                    return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("InvalidFeesPaidBy", language));
                }

                // Prevent overdrawing balances
                if (decimal.TryParse(contract.BuyerPayableAmount, out var buyerBalance) && buyerBalance < buyerPayableAmount)
                    return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("BuyerDeductionExceeds", language));

                if (decimal.TryParse(contract.SellerPayableAmount, out var sellerBalance) && sellerBalance < sellerPayableAmount)
                    return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("SellerDeductionExceeds", language));

                // Update contract amounts
                contract.BuyerPayableAmount = (buyerBalance - buyerPayableAmount).ToString("F2");
                contract.SellerPayableAmount = (sellerBalance - sellerPayableAmount).ToString("F2");
            }

            // Update milestone status
            if (milestone.Status == nameof(MilestoneStatus.Pending))
            {
                milestone.Status = nameof(MilestoneStatus.Escrow);
                contract.Status = nameof(ContractStatus.Accepted);
            }
            else if (milestone.Status == nameof(MilestoneStatus.Escrow))
            {
                milestone.Status = nameof(MilestoneStatus.Released);
            }

            // Check if all milestones are in escrow
            var allMilestonesInEscrow = await _context.MileStones
                .Where(m => m.ContractId == milestone.ContractId)
                .AllAsync(m => m.Status == nameof(MilestoneStatus.Escrow) || m.Status == nameof(MilestoneStatus.Released), cancellationToken);

            if (allMilestonesInEscrow)
            {
                contract.Status = nameof(ContractStatus.Escrow);
                contract.EscrowStatusUpdatedAt = DateTime.UtcNow;
            }

            milestone.LastModified = DateTime.UtcNow;
            milestone.LastModifiedBy = userId;

            _context.ContractDetails.Update(contract);
            _context.MileStones.Update(milestone);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("MilestonesUpdated", language), new
            {
                MilestoneId = milestone.Id,
                NewStatus = milestone.Status,
                BuyerPayableAmount = contract.BuyerPayableAmount,
                SellerPayableAmount = contract.SellerPayableAmount
            });
        }
    }
}




































//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Enums;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.ContractPanel.MilestoneCommands;
//public class UpdateMilestoneStatusCommand : IRequest<Result<object>>
//{
//    public int MilestoneId { get; set; }
//}

//public class UpdateMilestoneStatusCommandHandler : IRequestHandler<UpdateMilestoneStatusCommand, Result<object>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;

//    public UpdateMilestoneStatusCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//    {
//        _context = context;
//        _jwtService = jwtService;
//    }

//    public async Task<Result<object>> Handle(UpdateMilestoneStatusCommand request, CancellationToken cancellationToken)
//    {
//        var milestone = await _context.MileStones
//            .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

//        if (milestone == null)
//            return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.NotFound);

//        var userId = _jwtService.GetUserId();

//        // Fetch the related contract
//        var contract = await _context.ContractDetails
//        .FirstOrDefaultAsync(c => c.Id == milestone.ContractId, cancellationToken);

//        if (contract == null)
//            return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Relatedcontractnotfound);

//        if (milestone.Status != nameof(MilestoneStatus.Escrow))
//        {
//            // Retrieve the commission master to calculate the commission and tax
//            var commission = await _context.CommissionMasters
//                .Where(c => c.AppliedGlobally && c.TransactionType == "Service")
//                .OrderByDescending(c => c.Id)
//                .FirstOrDefaultAsync(cancellationToken);

//            if (commission == null)
//                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Commissioninformationnotfound);

//            // Extract and calculate fee amount, escrow tax, and tax amount
//            decimal feeAmount = milestone.Amount;
//            decimal escrowTax = commission.CommissionRate;
//            decimal taxRate = commission.TaxRate;

//            decimal escrowAmount = (feeAmount * escrowTax) / 100;
//            decimal taxAmount = (escrowAmount * taxRate) / 100;
//            decimal totalAmount = feeAmount + taxAmount + escrowAmount;

//            // Decide who pays the fee
//            decimal buyerPayableAmount = 0;
//            decimal sellerPayableAmount = 0;

//            if (contract.FeesPaidBy.ToLower() == "buyer")
//            {
//                buyerPayableAmount = feeAmount + escrowAmount;
//                sellerPayableAmount = feeAmount - taxAmount;
//            }
//            else if (contract.FeesPaidBy.ToLower() == "seller")
//            {
//                sellerPayableAmount = feeAmount - taxAmount - escrowAmount;
//                buyerPayableAmount = feeAmount;
//            }
//            else if (contract.FeesPaidBy.ToLower() == "50" || contract.FeesPaidBy.ToLower() == "halfpayment")
//            {
//                buyerPayableAmount = feeAmount + (escrowAmount / 2);
//                sellerPayableAmount = feeAmount - taxAmount - (escrowAmount / 2);
//            }
//            else
//            {
//                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.InvalidFeesPaidBy);
//            }

//            // Prevent overdrawing balances
//            if (decimal.TryParse(contract.BuyerPayableAmount, out var buyerBalance) && buyerBalance < buyerPayableAmount)
//                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.BuyerDeductionexceeds);

//            if (decimal.TryParse(contract.SellerPayableAmount, out var sellerBalance) && sellerBalance < sellerPayableAmount)
//                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.SellerDeductionexceeds);

//            // Update contract amounts
//            contract.BuyerPayableAmount = (buyerBalance - buyerPayableAmount).ToString("F2");
//            contract.SellerPayableAmount = (sellerBalance - sellerPayableAmount).ToString("F2");

//        }
//        // Update milestone status
//        if (milestone.Status == nameof(MilestoneStatus.Pending))
//        {
//            milestone.Status = nameof(MilestoneStatus.Escrow);
//            contract.Status = nameof(ContractStatus.Accepted);
//        }
//        else if (milestone.Status == nameof(MilestoneStatus.Escrow))
//        {
//            milestone.Status = nameof(MilestoneStatus.Released);

//        }
//        // Check if all milestones are in escrow
//        var allMilestonesInEscrow = await _context.MileStones
//    .Where(m => m.ContractId == milestone.ContractId)
//    .AllAsync(m => m.Status == nameof(MilestoneStatus.Escrow) || m.Status == nameof(MilestoneStatus.Released), cancellationToken);

//        if (allMilestonesInEscrow)
//        {
//            contract.Status = nameof(ContractStatus.Escrow);
//            contract.EscrowStatusUpdatedAt = DateTime.UtcNow;
//        }

//        milestone.LastModified = DateTime.UtcNow;
//        milestone.LastModifiedBy = userId;

//        _context.ContractDetails.Update(contract);

//        _context.MileStones.Update(milestone);
//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Milestonesupdated, new
//        {
//            MilestoneId = milestone.Id,
//            NewStatus = milestone.Status,
//            BuyerPayableAmount = contract.BuyerPayableAmount,
//            SellerPayableAmount = contract.SellerPayableAmount
//        });
//    }
//}
