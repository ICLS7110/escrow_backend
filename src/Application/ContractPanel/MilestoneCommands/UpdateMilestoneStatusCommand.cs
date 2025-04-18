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

        // Retrieve the commission master to calculate the commission and tax
        var commission = await _context.CommissionMasters
            .Where(c => c.AppliedGlobally && c.TransactionType == "Service")
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (commission == null)
            return Result<object>.Failure(404, "Commission information not found.");

        // Extract and calculate fee amount, escrow tax, and tax amount
        decimal feeAmount = contract.FeeAmount ?? 0;
        decimal escrowTax = commission.CommissionRate ;
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
        contract.LastModified = DateTime.UtcNow;
        contract.LastModifiedBy = userId;
        _context.ContractDetails.Update(contract);

        // Update milestone status
        if (milestone.Status == nameof(MilestoneStatus.Pending))
        {
            milestone.Status = nameof(MilestoneStatus.Escrow);
        }
        else if (milestone.Status == nameof(MilestoneStatus.Escrow))
        {
            milestone.Status = nameof(MilestoneStatus.Released);
        }

        milestone.LastModified = DateTime.UtcNow;
        milestone.LastModifiedBy = userId;

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











































//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Enums;
//using MediatR;
//using Microsoft.EntityFrameworkCore;

//namespace Escrow.Api.Application.ContractPanel.MilestoneCommands
//{
//    public class UpdateMilestoneStatusCommand : IRequest<Result<object>>
//    {
//        public int MilestoneId { get; set; }
//    }

//    public class UpdateMilestoneStatusCommandHandler : IRequestHandler<UpdateMilestoneStatusCommand, Result<object>>
//    {
//        private readonly IApplicationDbContext _context;
//        private readonly IJwtService _jwtService;

//        public UpdateMilestoneStatusCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//        {
//            _context = context;
//            _jwtService = jwtService;
//        }

//        public async Task<Result<object>> Handle(UpdateMilestoneStatusCommand request, CancellationToken cancellationToken)
//        {
//            var milestone = await _context.MileStones
//                .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

//            if (milestone == null)
//                return Result<object>.Failure(404, "Milestone not found.");

//            var userId = _jwtService.GetUserId();
//            if (milestone.Status != nameof(MilestoneStatus.Escrow))
//            {
//                // Fetch the contract separately
//                var contract = await _context.ContractDetails.FirstOrDefaultAsync(c => c.Id == milestone.ContractId, cancellationToken);

//                if (contract == null)
//                    return Result<object>.Failure(404, "Associated contract not found.");



//                // Parse current buyer/seller payable
//                if (!decimal.TryParse(contract.BuyerPayableAmount, out var buyerPayable))
//                    buyerPayable = 0;

//                if (!decimal.TryParse(contract.SellerPayableAmount, out var sellerPayable))
//                    sellerPayable = 0;

//                // Parse milestone financial values
//                decimal milestoneAmount = milestone.Amount;
//                decimal milestoneEscrow = decimal.TryParse(milestone.MileStoneEscrowAmount, out var escrowVal) ? escrowVal : 0;
//                decimal milestoneTax = decimal.TryParse(milestone.MileStoneTaxAmount, out var taxVal) ? taxVal : 0;

//                decimal buyerDeduction = 0;
//                decimal sellerDeduction = 0;
//                var feesPaidBy = (contract.FeesPaidBy ?? "50").ToLower();

//                switch (feesPaidBy)
//                {
//                    case "buyer":
//                        buyerDeduction = milestoneAmount + milestoneEscrow;
//                        sellerDeduction = milestoneAmount - milestoneTax;
//                        break;

//                    case "seller":
//                        buyerDeduction = milestoneAmount;
//                        sellerDeduction = milestoneAmount - milestoneEscrow - milestoneTax;
//                        break;

//                    case "50":
//                    case "half payment":
//                    case "halfpayment":
//                        buyerDeduction = milestoneAmount + (milestoneEscrow / 2);
//                        sellerDeduction = milestoneAmount - (milestoneEscrow / 2) - milestoneTax;
//                        break;

//                    default:
//                        return Result<object>.Failure(400, "Invalid FeesPaidBy value in contract.");
//                }

//                // Prevent overdrawing balances
//                if (buyerPayable < buyerDeduction || sellerPayable < sellerDeduction)
//                    return Result<object>.Failure(400, "Deduction exceeds available payable amounts.");

//                // Update contract balances
//                contract.BuyerPayableAmount = (buyerPayable - buyerDeduction).ToString("F2");
//                contract.SellerPayableAmount = (sellerPayable - sellerDeduction).ToString("F2");
//                contract.LastModified = DateTime.UtcNow;
//                contract.LastModifiedBy = userId;
//                _context.ContractDetails.Update(contract);
//            }
//            // Update milestone status
//            //milestone.Status = newStatus.ToString();
//            if (milestone.Status == nameof(MilestoneStatus.Pending))
//            {
//                milestone.Status = nameof(MilestoneStatus.Escrow);
//            }
//            else if (milestone.Status == nameof(MilestoneStatus.Escrow))
//            {
//                milestone.Status = nameof(MilestoneStatus.Released);
//            }
//            //milestone.Status = newStatus.ToString();
//            milestone.LastModified = DateTime.UtcNow;
//            milestone.LastModifiedBy = userId;

//            _context.MileStones.Update(milestone);
//            await _context.SaveChangesAsync(cancellationToken);

//            return Result<object>.Success(200, "Milestone status updated successfully.", new
//            {
//                MilestoneId = milestone.Id,
//                NewStatus = milestone.Status,
//            });
//        }
//    }
//}
















































//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Enums;
//using MediatR;
//using Microsoft.EntityFrameworkCore;

//namespace Escrow.Api.Application.ContractPanel.MilestoneCommands
//{
//    public class UpdateMilestoneStatusCommand : IRequest<Result<object>>
//    {
//        public int MilestoneId { get; set; }
//        public string? Status { get; set; }
//    }

//    public class UpdateMilestoneStatusCommandHandler : IRequestHandler<UpdateMilestoneStatusCommand, Result<object>>
//    {
//        private readonly IApplicationDbContext _context;
//        private readonly IJwtService _jwtService;

//        public UpdateMilestoneStatusCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//        {
//            _context = context;
//            _jwtService = jwtService;
//        }

//        public async Task<Result<object>> Handle(UpdateMilestoneStatusCommand request, CancellationToken cancellationToken)
//        {
//            var milestone = await _context.MileStones
//                .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

//            if (milestone == null)
//                return Result<object>.Failure(404, "Milestone not found.");

//            if (!Enum.TryParse<MilestoneStatus>(request.Status, true, out var newStatus))
//                return Result<object>.Failure(400, "Invalid milestone status.");

//            var userId = _jwtService.GetUserId();

//            milestone.Status = newStatus.ToString();
//            milestone.LastModified = DateTime.UtcNow;
//            milestone.LastModifiedBy = userId;

//            // ✅ Fetch the contract separately
//            var contract = await _context.ContractDetails
//                .FirstOrDefaultAsync(c => c.Id == milestone.ContractId, cancellationToken);

//            if (contract == null)
//                return Result<object>.Failure(404, "Associated contract not found.");

//            // ✅ Get latest commission and tax rates
//            var commissionMaster = await _context.CommissionMasters
//                .Where(c => c.AppliedGlobally && c.TransactionType == "Service")
//                .OrderByDescending(c => c.Id)
//                .FirstOrDefaultAsync(cancellationToken);

//            if (commissionMaster != null && milestone.Amount > 0)
//            {
//                decimal amount = milestone.Amount;
//                decimal commissionRate = commissionMaster.CommissionRate;
//                decimal taxRate = commissionMaster.TaxRate;

//                decimal commissionAmount = Math.Round((amount * commissionRate) / 100, 2);
//                decimal taxAmount = Math.Round((commissionAmount * taxRate) / 100, 2);
//                decimal buyerPays = 0;
//                decimal sellerReceives = 0;

//                string feesPaidBy = contract.FeesPaidBy?.ToLower() ?? "buyer";

//                if (feesPaidBy == "buyer")
//                {
//                    buyerPays = amount + commissionAmount;
//                    sellerReceives = amount - taxAmount;
//                }
//                else if (feesPaidBy == "seller")
//                {
//                    buyerPays = amount;
//                    sellerReceives = amount - commissionAmount - taxAmount;
//                }
//                else if (feesPaidBy == "50" || feesPaidBy == "half payment")
//                {
//                    var halfCommission = commissionAmount / 2;
//                    buyerPays = amount + halfCommission;
//                    sellerReceives = amount - halfCommission - taxAmount;
//                }

//                contract.EscrowTax = commissionAmount;
//                contract.TaxAmount = taxAmount;
//                contract.BuyerPayableAmount = Math.Round(buyerPays, 2).ToString();
//                contract.SellerPayableAmount = Math.Round(sellerReceives, 2).ToString();
//            }

//            _context.MileStones.Update(milestone);
//            await _context.SaveChangesAsync(cancellationToken);

//            return Result<object>.Success(200, "Milestone status updated successfully.", new
//            {
//                MilestoneId = milestone.Id,
//                NewStatus = milestone.Status,
//                CommissionAmount = contract.EscrowTax,
//                TaxAmount = contract.TaxAmount,
//                BuyerPayableAmount = contract.BuyerPayableAmount,
//                SellerReceivableAmount = contract.SellerPayableAmount
//            });
//        }
//    }
//}









































////using System;
////using System.Threading;
////using System.Threading.Tasks;
////using Escrow.Api.Application.Common.Interfaces;
////using Escrow.Api.Application.Common.Models;
////using Escrow.Api.Application.DTOs;
////using Escrow.Api.Domain.Enums;
////using MediatR;
////using Microsoft.EntityFrameworkCore;

////namespace Escrow.Api.Application.ContractPanel.MilestoneCommands
////{
////    public class UpdateMilestoneStatusCommand : IRequest<Result<object>>
////    {
////        public int MilestoneId { get; set; }
////        public string? Status { get; set; } // Enum as string
////    }

////    public class UpdateMilestoneStatusCommandHandler : IRequestHandler<UpdateMilestoneStatusCommand, Result<object>>
////    {
////        private readonly IApplicationDbContext _context;
////        private readonly IJwtService _jwtService;

////        public UpdateMilestoneStatusCommandHandler(IApplicationDbContext context, IJwtService jwtService)
////        {
////            _context = context;
////            _jwtService = jwtService;
////        }

////        public async Task<Result<object>> Handle(UpdateMilestoneStatusCommand request, CancellationToken cancellationToken)
////        {
////            var milestone = await _context.MileStones
////                .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

////            if (milestone == null)
////                return Result<object>.Failure(404, "Milestone not found.");

////            if (!Enum.TryParse<MilestoneStatus>(request.Status, true, out var newStatus))
////                return Result<object>.Failure(400, "Invalid milestone status.");

////            var userId = _jwtService.GetUserId();

////            var contract = await _context.ContractDetails
////                .FirstOrDefaultAsync(c => c.Id == milestone.ContractId, cancellationToken);

////            if (contract == null)
////                return Result<object>.Failure(404, "Related contract not found.");

////            if (!decimal.TryParse(contract.BuyerPayableAmount, out var buyerPayable) ||
////                !decimal.TryParse(contract.SellerPayableAmount, out var sellerPayable))
////                return Result<object>.Failure(400, "Invalid buyer or seller payable amount.");

////            decimal milestoneAmount = milestone.Amount;
////            decimal escrowTax = contract.EscrowTax ?? 0;
////            decimal taxAmount = contract.TaxAmount ?? 0;
////            decimal totalContractFee = contract.FeeAmount ?? 0;

////            decimal escrowRate = totalContractFee == 0 ? 0 : escrowTax / totalContractFee;
////            decimal taxRate = totalContractFee == 0 ? 0 : taxAmount / totalContractFee;

////            decimal milestoneEscrow = milestoneAmount * escrowRate;
////            decimal milestoneTax = milestoneAmount * taxRate;

////            decimal buyerDeduction = 0;
////            decimal sellerDeduction = 0;

////            switch (contract.FeesPaidBy.ToLower())
////            {
////                case "buyer":
////                    buyerDeduction = milestoneAmount + milestoneEscrow;
////                    sellerDeduction = milestoneAmount - milestoneTax;
////                    break;

////                case "seller":
////                    buyerDeduction = milestoneAmount;
////                    sellerDeduction = milestoneAmount - milestoneTax - milestoneEscrow;
////                    break;

////                case "50":
////                case "halfpayment":
////                    buyerDeduction = milestoneAmount + (milestoneEscrow / 2);
////                    sellerDeduction = milestoneAmount - milestoneTax - (milestoneEscrow / 2);
////                    break;

////                default:
////                    return Result<object>.Failure(400, "Invalid FeesPaidBy value.");
////            }

////            if (buyerPayable < buyerDeduction || sellerPayable < sellerDeduction)
////                return Result<object>.Failure(400, "Milestone amount exceeds remaining payable amount.");

////            contract.BuyerPayableAmount = (buyerPayable - buyerDeduction).ToString("F2");
////            contract.SellerPayableAmount = (sellerPayable - sellerDeduction).ToString("F2");
////            contract.LastModified = DateTime.UtcNow;
////            contract.LastModifiedBy = userId;
////            _context.ContractDetails.Update(contract);

////            milestone.Status = newStatus.ToString();
////            milestone.LastModified = DateTime.UtcNow;
////            milestone.LastModifiedBy = userId;
////            _context.MileStones.Update(milestone);

////            await _context.SaveChangesAsync(cancellationToken);

////            return Result<object>.Success(200, "Milestone status and amounts updated successfully.", new
////            {
////                MilestoneId = milestone.Id,
////                NewStatus = milestone.Status,
////                BuyerPayableAmount = contract.BuyerPayableAmount,
////                SellerPayableAmount = contract.SellerPayableAmount
////            });
////        }
////    }

////}








//////using System;
//////using System.Threading;
//////using System.Threading.Tasks;
//////using Escrow.Api.Application.Common.Interfaces;
//////using Escrow.Api.Application.Common.Models;
//////using Escrow.Api.Application.DTOs;
//////using Escrow.Api.Domain.Enums;
//////using MediatR;
//////using Microsoft.EntityFrameworkCore;

//////namespace Escrow.Api.Application.ContractPanel.MilestoneCommands
//////{
//////    public class UpdateMilestoneStatusCommand : IRequest<Result<object>>
//////    {
//////        public int MilestoneId { get; set; }
//////        public string? Status { get; set; } // Enum as string
//////    }

//////    public class UpdateMilestoneStatusCommandHandler : IRequestHandler<UpdateMilestoneStatusCommand, Result<object>>
//////    {
//////        private readonly IApplicationDbContext _context;
//////        private readonly IJwtService _jwtService;

//////        public UpdateMilestoneStatusCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//////        {
//////            _context = context;
//////            _jwtService = jwtService;
//////        }

//////        public async Task<Result<object>> Handle(UpdateMilestoneStatusCommand request, CancellationToken cancellationToken)
//////        {
//////            var milestone = await _context.MileStones
//////                .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

//////            if (milestone == null)
//////            {
//////                return Result<object>.Failure(404, "Milestone not found.");
//////            }

//////            if (!Enum.TryParse<MilestoneStatus>(request.Status, true, out var newStatus))
//////            {
//////                return Result<object>.Failure(400, "Invalid milestone status.");
//////            }

//////            var userId = _jwtService.GetUserId();

//////            // Fetch the related contract
//////            var contract = await _context.ContractDetails
//////                .FirstOrDefaultAsync(c => c.Id == milestone.ContractId, cancellationToken);

//////            if (contract == null)
//////            {
//////                return Result<object>.Failure(404, "Related contract not found.");
//////            }

//////            // Subtract milestone amount from the contract total (if applicable)
//////            if (milestone.Amount > 0)
//////            {
//////                contract.BuyerPayableAmount = Convert.ToString(Convert.ToDecimal(contract.BuyerPayableAmount) - Convert.ToDecimal(milestone.Amount));
//////                contract.SellerPayableAmount = Convert.ToString(Convert.ToDecimal(contract.SellerPayableAmount) - Convert.ToDecimal(milestone.Amount));
//////                contract.LastModified = DateTime.UtcNow;
//////                contract.LastModifiedBy = userId;
//////                _context.ContractDetails.Update(contract);
//////            }

//////            milestone.Status = newStatus.ToString();
//////            milestone.LastModified = DateTime.UtcNow;
//////            milestone.LastModifiedBy = userId;

//////            _context.MileStones.Update(milestone);
//////            await _context.SaveChangesAsync(cancellationToken);

//////            return Result<object>.Success(200, "Milestone status and contract amount updated successfully.", new
//////            {
//////                MilestoneId = milestone.Id,
//////                NewStatus = milestone.Status,
//////                BuyerPayableAmount = contract.BuyerPayableAmount,
//////                SellerPayableAmount = contract.SellerPayableAmount
//////            });
//////        }

//////    }
//////}


















//////using System;
//////using System.Threading;
//////using System.Threading.Tasks;
//////using Escrow.Api.Application.Common.Interfaces;
//////using Escrow.Api.Application.Common.Models;
//////using Escrow.Api.Application.DTOs;
//////using Escrow.Api.Domain.Enums;
//////using MediatR;
//////using Microsoft.EntityFrameworkCore;

//////namespace Escrow.Api.Application.ContractPanel.MilestoneCommands
//////{
//////    public class UpdateMilestoneStatusCommand : IRequest<Result<object>>
//////    {
//////        public int MilestoneId { get; set; }
//////        public string? Status { get; set; } // Enum as string
//////    }

//////    public class UpdateMilestoneStatusCommandHandler : IRequestHandler<UpdateMilestoneStatusCommand, Result<object>>
//////    {
//////        private readonly IApplicationDbContext _context;
//////        private readonly IJwtService _jwtService;

//////        public UpdateMilestoneStatusCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//////        {
//////            _context = context;
//////            _jwtService = jwtService;
//////        }

//////        public async Task<Result<object>> Handle(UpdateMilestoneStatusCommand request, CancellationToken cancellationToken)
//////        {
//////            var milestone = await _context.MileStones
//////                .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

//////            if (milestone == null)
//////            {
//////                return Result<object>.Failure(404, "Milestone not found.");
//////            }

//////            if (!Enum.TryParse<MilestoneStatus>(request.Status, true, out var newStatus))
//////            {
//////                return Result<object>.Failure(400, "Invalid milestone status.");
//////            }

//////            var userId = _jwtService.GetUserId();

//////            milestone.Status = nameof(newStatus);
//////            milestone.LastModified = DateTime.UtcNow;
//////            milestone.LastModifiedBy = userId;

//////            _context.MileStones.Update(milestone);
//////            await _context.SaveChangesAsync(cancellationToken);

//////            return Result<object>.Success(200, "Milestone status updated successfully.", new
//////            {
//////                MilestoneId = milestone.Id,
//////                NewStatus = milestone.Status.ToString()
//////            });
//////        }
//////    }
//////}
