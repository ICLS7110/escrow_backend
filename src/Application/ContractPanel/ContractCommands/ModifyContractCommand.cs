using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Escrow.Api.Application.ContractPanel.ContractCommands;
public class ModifyContractCommand : IRequest<Result<object>>
{
    public int? ContractId { get; set; }
    public string? Role { get; set; }
    public string? ContractTitle { get; set; }
    public string? ServiceType { get; set; }
    public string? ServiceDescription { get; set; }
    public string? AdditionalNote { get; set; }
    public string? FeesPaidBy { get; set; }
    public decimal FeeAmount { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerMobile { get; set; }
    public string? SellerName { get; set; }
    public string? SellerMobile { get; set; }
    public string? ContractDoc { get; set; }
    public string? Status { get; set; }
    public int InvitationId { get; set; }
    public List<MileStoneDTO> MileStoneDetails { get; set; } = new();
}



public class ModifyContractCommandHandler : IRequestHandler<ModifyContractCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ModifyContractCommandHandler(IApplicationDbContext context, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<object>> Handle(ModifyContractCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the current language (defaults to English if none provided)
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;
            int userId = _jwtService.GetUserId().ToInt();

            // Find contract by ID
            var contract = await _context.ContractDetails
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            // If contract not found, return failure with language-specific message
            if (contract == null)
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("ContractNotFound", language));

            var oldData = JsonConvert.SerializeObject(contract, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            // Update contract base fields
            contract.Role = request.Role ?? string.Empty;
            contract.ContractTitle = request.ContractTitle ?? string.Empty;
            contract.ServiceType = request.ServiceType ?? string.Empty;
            contract.ServiceDescription = request.ServiceDescription ?? string.Empty;
            contract.AdditionalNote = request.AdditionalNote ?? string.Empty;
            contract.FeesPaidBy = request.FeesPaidBy ?? string.Empty;
            contract.FeeAmount = request.FeeAmount;
            contract.BuyerName = request.BuyerName ?? string.Empty;
            contract.BuyerMobile = request.BuyerMobile ?? string.Empty;
            contract.SellerName = request.SellerName ?? string.Empty;
            contract.SellerMobile = request.SellerMobile ?? string.Empty;
            contract.ContractDoc = request.ContractDoc ?? string.Empty;
            contract.Status = nameof(ContractStatus.Pending);
            contract.LastModified = DateTime.UtcNow;
            contract.LastModifiedBy = userId.ToString();

            // --- Begin calculation for escrow, tax, buyer/seller payable amounts ---
            var commission = await _context.CommissionMasters
                .Where(c => c.AppliedGlobally || c.TransactionType == "Service")
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync(cancellationToken);

            decimal feeAmount = request.FeeAmount;
            decimal escrowTax = commission?.CommissionRate ?? 0;
            decimal taxRate = commission?.TaxRate ?? 0;
            decimal escrowAmount = (feeAmount * escrowTax) / 100;
            decimal taxAmount = (escrowAmount * taxRate) / 100;
            decimal buyerPayableAmount = 0;
            decimal sellerPayableAmount = 0;

            if (request?.FeesPaidBy?.ToLower() == "buyer")
            {
                buyerPayableAmount = feeAmount + escrowAmount + taxAmount;
                sellerPayableAmount = feeAmount;
            }
            else if (request?.FeesPaidBy?.ToLower() == "seller")
            {
                sellerPayableAmount = feeAmount - taxAmount - escrowAmount;
                buyerPayableAmount = feeAmount;
            }
            else if (request?.FeesPaidBy?.ToLower() == "50" || request?.FeesPaidBy?.ToLower() == "halfPayment")
            {
                buyerPayableAmount = feeAmount + (taxAmount / 2) + (escrowAmount / 2);
                sellerPayableAmount = feeAmount - (taxAmount / 2) - (escrowAmount / 2);
            }

            contract.EscrowTax = escrowAmount;
            contract.TaxAmount = taxAmount;
            contract.BuyerPayableAmount = buyerPayableAmount.ToString();
            contract.SellerPayableAmount = sellerPayableAmount.ToString();
            // --- End of calculation logic ---

            // Handle milestone updates
            var existingMilestones = await _context.MileStones
                .Where(x => x.ContractId == contract.Id)
                .ToListAsync(cancellationToken);

            var requestMilestones = request?.MileStoneDetails ?? new List<MileStoneDTO>();

            foreach (var dto in requestMilestones)
            {
                var existing = existingMilestones.FirstOrDefault(x => x.Id == dto.Id);

                if (existing != null)
                {
                    existing.Name = dto.Name;
                    existing.Amount = dto.Amount;
                    existing.Description = dto.Description;
                    existing.DueDate = dto.DueDate;
                    existing.Documents = dto.Documents;
                    existing.Status = nameof(MilestoneStatus.Pending);
                    existing.MileStoneEscrowAmount = dto.MileStoneEscrowAmount;
                    existing.MileStoneTaxAmount = dto.MileStoneTaxAmount;
                    existing.LastModifiedBy = userId.ToString();
                    existing.LastModified = DateTime.UtcNow;

                    _context.MileStones.Update(existing);
                }
                else
                {
                    var newMilestone = new MileStone
                    {
                        ContractId = contract.Id,
                        Name = dto.Name,
                        Amount = dto.Amount,
                        Description = dto.Description,
                        DueDate = dto.DueDate,
                        Documents = dto.Documents,
                        Status = nameof(MilestoneStatus.Pending),
                        MileStoneEscrowAmount = dto.MileStoneEscrowAmount,
                        MileStoneTaxAmount = dto.MileStoneTaxAmount,
                        Created = DateTime.UtcNow,
                        CreatedBy = userId.ToString()
                    };

                    await _context.MileStones.AddAsync(newMilestone, cancellationToken);
                }
            }

            // Handle invitation update
            if (request?.InvitationId > 0)
            {
                var invitation = await _context.SellerBuyerInvitations
                    .FirstOrDefaultAsync(i => i.ContractId == contract.Id && i.Id == request.InvitationId, cancellationToken);

                if (invitation != null)
                {
                    invitation.Status = "Updated";
                    invitation.SellerPhoneNumber = request.SellerMobile ?? invitation.SellerPhoneNumber;
                    invitation.BuyerPhoneNumber = request.BuyerMobile ?? invitation.BuyerPhoneNumber;
                    _context.SellerBuyerInvitations.Update(invitation);
                }
                else
                {
                    return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("Invitationnotfound", language));
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            var newData = JsonConvert.SerializeObject(contract, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            var log = new ContractDetailsLog
            {
                ContractId = contract.Id,
                Operation = "UPDATE",
                ChangedFields = "All",
                PreviousData = oldData,
                NewData = newData,
                Remarks = "Contract updated by user.",
                Created = DateTime.UtcNow,
                ChangedBy = userId.ToString(),
                Source = "API"
            };

            await _context.ContractDetailsLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Return success message with dynamic language support
            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("ContractUpdated", language), new { contract.Id });
        }
        catch (Exception ex)
        {
            // Return failure with error message
            return Result<object>.Failure(StatusCodes.Status500InternalServerError, $"Error updating contract: {ex.Message}");
        }
    }
}





//public class ModifyContractCommandHandler : IRequestHandler<ModifyContractCommand, Result<object>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;

//    public ModifyContractCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//    {
//        _context = context;
//        _jwtService = jwtService;
//    }

//    public async Task<Result<object>> Handle(ModifyContractCommand request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            int userId = _jwtService.GetUserId().ToInt();

//            var contract = await _context.ContractDetails
//                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

//            if (contract == null)
//                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.ContractNotFound);


//    //        var monthlyLimitConfig = await _context.SystemConfigurations.FirstOrDefaultAsync(cancellationToken);
//    //        decimal monthlyLimit = Convert.ToDecimal(monthlyLimitConfig?.Value ?? "0");

//    //        DateTime startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
//    //        DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

//    //        decimal totalFeeAmountThisMonth = await _context.ContractDetails
//    //.Where(c => c.CreatedBy == Convert.ToString(userId) &&
//    //            c.Created >= startOfMonth &&
//    //            c.Created <= endOfMonth)
//    //.SumAsync(c => (decimal?)c.FeeAmount, cancellationToken) ?? 0m;


//    //        decimal newFeeAmount = Convert.ToDecimal(request.FeeAmount.ToString() ?? "0");
//    //        if ((totalFeeAmountThisMonth + newFeeAmount) > monthlyLimit)
//    //        {
//    //            throw new InvalidOperationException("You have exceeded your monthly contract creation limit.");
//    //            // OR return Result<int>.Failure(StatusCodes.Status400BadRequest, "Monthly limit exceeded.");
//    //        }

//            var oldData = JsonConvert.SerializeObject(contract, new JsonSerializerSettings
//            {
//                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
//            });

//            // Update contract base fields
//            contract.Role = request.Role ?? string.Empty;
//            contract.ContractTitle = request.ContractTitle ?? string.Empty;
//            contract.ServiceType = request.ServiceType ?? string.Empty;
//            contract.ServiceDescription = request.ServiceDescription ?? string.Empty;
//            contract.AdditionalNote = request.AdditionalNote ?? string.Empty;
//            contract.FeesPaidBy = request.FeesPaidBy ?? string.Empty;
//            contract.FeeAmount = request.FeeAmount;
//            contract.BuyerName = request.BuyerName ?? string.Empty;
//            contract.BuyerMobile = request.BuyerMobile ?? string.Empty;
//            contract.SellerName = request.SellerName ?? string.Empty;
//            contract.SellerMobile = request.SellerMobile ?? string.Empty;
//            contract.ContractDoc = request.ContractDoc ?? string.Empty;
//            contract.Status = nameof(ContractStatus.Pending);
//            contract.LastModified = DateTime.UtcNow;
//            contract.LastModifiedBy = userId.ToString();

//            // --- Begin calculation for escrow, tax, buyer/seller payable amounts ---
//            var commission = await _context.CommissionMasters
//                .Where(c => c.AppliedGlobally || c.TransactionType == "Service")
//                .OrderByDescending(c => c.Id)
//                .FirstOrDefaultAsync(cancellationToken);

//            decimal feeAmount = request.FeeAmount;
//            decimal escrowTax = commission?.CommissionRate ?? 0;
//            decimal taxRate = commission?.TaxRate ?? 0;
//            decimal escrowAmount = (feeAmount * escrowTax) / 100;
//            decimal taxAmount = (escrowAmount * taxRate) / 100;
//            decimal buyerPayableAmount = 0;
//            decimal sellerPayableAmount = 0;


//            if (request?.FeesPaidBy?.ToLower() == "buyer")
//            {
//                buyerPayableAmount = feeAmount + escrowAmount + taxAmount;
//                sellerPayableAmount = feeAmount;
//            }
//            else if (request?.FeesPaidBy?.ToLower() == "seller")
//            {
//                sellerPayableAmount = feeAmount - taxAmount - escrowAmount;
//                buyerPayableAmount = feeAmount;
//            }
//            else if (request?.FeesPaidBy?.ToLower() == "50" || request?.FeesPaidBy?.ToLower() == "halfPayment")
//            {
//                buyerPayableAmount = feeAmount + (taxAmount / 2) + (escrowAmount / 2);
//                sellerPayableAmount = feeAmount - (taxAmount / 2) - (escrowAmount / 2);
//            }

//            contract.EscrowTax = escrowAmount;
//            contract.TaxAmount = taxAmount;
//            contract.BuyerPayableAmount = buyerPayableAmount.ToString();
//            contract.SellerPayableAmount = sellerPayableAmount.ToString();
//            // --- End of calculation logic ---

//            // Handle milestone updates
//            var existingMilestones = await _context.MileStones
//                .Where(x => x.ContractId == contract.Id)
//                .ToListAsync(cancellationToken);

//            var requestMilestones = request?.MileStoneDetails ?? new List<MileStoneDTO>();

//            foreach (var dto in requestMilestones)
//            {
//                var existing = existingMilestones.FirstOrDefault(x => x.Id == dto.Id);

//                if (existing != null)
//                {
//                    existing.Name = dto.Name;
//                    existing.Amount = dto.Amount;
//                    existing.Description = dto.Description;
//                    existing.DueDate = dto.DueDate;
//                    existing.Documents = dto.Documents;
//                    existing.Status = nameof(MilestoneStatus.Pending);
//                    existing.MileStoneEscrowAmount = dto.MileStoneEscrowAmount;
//                    existing.MileStoneTaxAmount = dto.MileStoneTaxAmount;
//                    existing.LastModifiedBy = userId.ToString();
//                    existing.LastModified = DateTime.UtcNow;

//                    _context.MileStones.Update(existing);
//                }
//                else
//                {
//                    var newMilestone = new MileStone
//                    {
//                        ContractId = contract.Id,
//                        Name = dto.Name,
//                        Amount = dto.Amount,
//                        Description = dto.Description,
//                        DueDate = dto.DueDate,
//                        Documents = dto.Documents,
//                        Status = nameof(MilestoneStatus.Pending),
//                        MileStoneEscrowAmount = dto.MileStoneEscrowAmount,
//                        MileStoneTaxAmount = dto.MileStoneTaxAmount,
//                        Created = DateTime.UtcNow,
//                        CreatedBy = userId.ToString()
//                    };

//                    await _context.MileStones.AddAsync(newMilestone, cancellationToken);
//                }
//            }

//            // Handle invitation update
//            if (request?.InvitationId > 0)
//            {
//                var invitation = await _context.SellerBuyerInvitations
//                    .FirstOrDefaultAsync(i => i.ContractId == contract.Id && i.Id == request.InvitationId, cancellationToken);

//                if (invitation != null)
//                {
//                    invitation.Status = "Updated";
//                    invitation.SellerPhoneNumber = request.SellerMobile ?? invitation.SellerPhoneNumber;
//                    invitation.BuyerPhoneNumber = request.BuyerMobile ?? invitation.BuyerPhoneNumber;
//                    _context.SellerBuyerInvitations.Update(invitation);
//                }
//                else
//                {
//                    return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Invitationnotfound);
//                }
//            }

//            await _context.SaveChangesAsync(cancellationToken);

//            var newData = JsonConvert.SerializeObject(contract, new JsonSerializerSettings
//            {
//                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
//            });

//            var log = new ContractDetailsLog
//            {
//                ContractId = contract.Id,
//                Operation = "UPDATE",
//                ChangedFields = "All",
//                PreviousData = oldData,
//                NewData = newData,
//                Remarks = "Contract updated by user.",
//                Created = DateTime.UtcNow,
//                ChangedBy = userId.ToString(),
//                Source = "API"
//            };

//            await _context.ContractDetailsLogs.AddAsync(log, cancellationToken);
//            await _context.SaveChangesAsync(cancellationToken);

//            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.ContractUpdated, new { contract.Id });
//        }
//        catch (Exception ex)
//        {
//            return Result<object>.Failure(StatusCodes.Status500InternalServerError, $"Error updating contract: {ex.Message}");
//        }
//    }
//}
