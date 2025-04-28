using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.ContractPanel.MilestoneCommands;
public class UpdateMilestoneStatusCommand : IRequest<Result<object>>
{
    public int MilestoneId { get; set; }
}

public class UpdateMilestoneStatusCommandHandler : IRequestHandler<UpdateMilestoneStatusCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public UpdateMilestoneStatusCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<object>> Handle(UpdateMilestoneStatusCommand request, CancellationToken cancellationToken)
    {
        var milestone = await _context.MileStones
            .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

        if (milestone == null)
            return Result<object>.Failure(404, "Milestone not found.");

        var userId = _jwtService.GetUserId();

        // Fetch the related contract
        var contract = await _context.ContractDetails
        .FirstOrDefaultAsync(c => c.Id == milestone.ContractId, cancellationToken);

        if (contract == null)
            return Result<object>.Failure(404, "Related contract not found.");

        if (milestone.Status != nameof(MilestoneStatus.Escrow))
        {
            // Retrieve the commission master to calculate the commission and tax
            var commission = await _context.CommissionMasters
                .Where(c => c.AppliedGlobally && c.TransactionType == "Service")
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (commission == null)
                return Result<object>.Failure(404, "Commission information not found.");

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
                return Result<object>.Failure(400, "Invalid FeesPaidBy value.");
            }

            // Prevent overdrawing balances
            if (decimal.TryParse(contract.BuyerPayableAmount, out var buyerBalance) && buyerBalance < buyerPayableAmount)
                return Result<object>.Failure(400, "Deduction exceeds available buyer balance.");

            if (decimal.TryParse(contract.SellerPayableAmount, out var sellerBalance) && sellerBalance < sellerPayableAmount)
                return Result<object>.Failure(400, "Deduction exceeds available seller balance.");

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
        }

        milestone.LastModified = DateTime.UtcNow;
        milestone.LastModifiedBy = userId;

        _context.ContractDetails.Update(contract);

        _context.MileStones.Update(milestone);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object>.Success(200, "Milestone status updated successfully.", new
        {
            MilestoneId = milestone.Id,
            NewStatus = milestone.Status,
            BuyerPayableAmount = contract.BuyerPayableAmount,
            SellerPayableAmount = contract.SellerPayableAmount
        });
    }
}
