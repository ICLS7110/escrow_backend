using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.ContractPanel;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.ContractPanel.ContractCommands
{
    public record EditContractDetailCommand : IRequest<Result<int>>
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string ContractTitle { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public string? AdditionalNote { get; set; }
        public string FeesPaidBy { get; set; } = string.Empty;
        public decimal? FeeAmount { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerMobile { get; set; }
        public string? SellerName { get; set; }
        public string? SellerMobile { get; set; }
        public string? ContractDoc { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class EditContractDetailCommandHandler : IRequestHandler<EditContractDetailCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EditContractDetailCommandHandler(IApplicationDbContext applicationDbContext, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
        {
            _context = applicationDbContext;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<int>> Handle(EditContractDetailCommand request, CancellationToken cancellationToken)
        {
            int userid = _jwtService.GetUserId().ToInt();

            // Get the current language from HttpContext
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            var entity = await _context.ContractDetails
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                return Result<int>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("ContractNotFound", language));
            }
            var commission = await _context.CommissionMasters
               .Where(c => c.AppliedGlobally == true)
               .OrderByDescending(c => c.Id)
               .FirstOrDefaultAsync(cancellationToken);

            decimal feeAmount = request.FeeAmount ?? 0;
            decimal escrowTax = commission?.CommissionRate ?? 0;
            decimal taxRate = commission?.TaxRate ?? 0;
            decimal escrowAmount = 0;
            decimal taxAmount = 0;
            decimal buyerPayableAmount = 0;
            decimal sellerPayableAmount = 0;

            switch (request.FeesPaidBy.ToLower())
            {
                case "buyer":
                    escrowAmount = (feeAmount * escrowTax) / 100;
                    taxAmount = (escrowAmount * taxRate) / 100;
                    buyerPayableAmount = feeAmount + escrowAmount + taxAmount;
                    sellerPayableAmount = feeAmount;
                    break;
                case "seller":
                    escrowAmount = (feeAmount * escrowTax) / 100;
                    taxAmount = (escrowAmount * taxRate) / 100;
                    sellerPayableAmount = feeAmount - taxAmount - escrowAmount;
                    buyerPayableAmount = feeAmount;
                    break;
                case "50":
                case "halfpayment":

                    escrowAmount = (feeAmount * escrowTax) / 100;
                    taxAmount = (escrowAmount * taxRate) / 100;
                    escrowAmount /= 2;
                    taxAmount /= 2;
                    buyerPayableAmount = feeAmount + (taxAmount) + (escrowAmount);
                    sellerPayableAmount = feeAmount - (taxAmount) - (escrowAmount);
                    break;
            }

            // Capture the old data snapshot as JSON
            var oldData = new
            {
                entity.Id,
                entity.Role,
                entity.ContractTitle,
                entity.ServiceType,
                entity.ServiceDescription,
                entity.AdditionalNote,
                entity.FeesPaidBy,
                entity.FeeAmount,
                entity.BuyerName,
                entity.BuyerMobile,
                entity.SellerName,
                entity.SellerMobile,
                entity.ContractDoc,
                entity.Status,
                entity.BuyerDetailsId,
                entity.SellerDetailsId
            };

            // Update entity
            entity.Role = request.Role;
            entity.TaxAmount = taxAmount;
            entity.EscrowTax = escrowAmount;
            entity.BuyerPayableAmount = $"{buyerPayableAmount}";
            entity.SellerPayableAmount = $"{sellerPayableAmount}";
            entity.ContractTitle = request.ContractTitle;
            entity.ServiceType = request.ServiceType;
            entity.ServiceDescription = request.ServiceDescription;
            entity.AdditionalNote = request.AdditionalNote;
            entity.FeesPaidBy = request.FeesPaidBy;
            entity.FeeAmount = feeAmount;
            entity.BuyerName = request.BuyerName;
            entity.BuyerMobile = request.BuyerMobile;
            entity.SellerMobile = request.SellerMobile;
            entity.SellerName = request.SellerName;
            entity.Status = request.Status;
            entity.ContractDoc = request.ContractDoc;
            entity.BuyerDetailsId = request.Role == EscrowApIConstant.ContratConstant.ContractRoleBuyer ? userid : null;
            entity.SellerDetailsId = request.Role == EscrowApIConstant.ContratConstant.ContractRoleSeller ? userid : null;

            // Capture the new data snapshot as JSON
            var newData = new
            {
                entity.Id,
                entity.Role,
                entity.ContractTitle,
                entity.ServiceType,
                entity.ServiceDescription,
                entity.AdditionalNote,
                entity.FeesPaidBy,
                entity.FeeAmount,
                entity.BuyerName,
                entity.BuyerMobile,
                entity.SellerName,
                entity.SellerMobile,
                entity.ContractDoc,
                entity.Status,
                entity.BuyerDetailsId,
                entity.SellerDetailsId
            };

            // Create the log entry
            var log = new ContractDetailsLog
            {
                ContractId = entity.Id,
                Operation = "UPDATE",
                ChangedFields = "ALL", // Optional: You can compare and list changed fields here if needed
                PreviousData = System.Text.Json.JsonSerializer.Serialize(oldData),
                NewData = System.Text.Json.JsonSerializer.Serialize(newData),
                Remarks = "Full update from Edit API",
                Created = DateTime.UtcNow,
                ChangedBy = userid.ToString(),
                Source = "API"
            };

            await _context.ContractDetailsLogs.AddAsync(log, cancellationToken);
            _context.ContractDetails.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(StatusCodes.Status200OK, AppMessages.Get("Success", language), entity.Id);
        }
    }
}




























//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.Common.Models.ContractDTOs;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Entities.ContractPanel;
//using Escrow.Api.Domain.Entities.UserPanel;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.ContractPanel.ContractCommands;

//public record EditContractDetailCommand : IRequest<Result<int>>
//{
//    public int Id { get; set; }
//    public string Role { get; set; } = string.Empty;
//    public string ContractTitle { get; set; } = string.Empty;
//    public string ServiceType { get; set; } = string.Empty;
//    public string ServiceDescription { get; set; } = string.Empty;
//    public string? AdditionalNote { get; set; }
//    public string FeesPaidBy { get; set; } = string.Empty;
//    public decimal? FeeAmount { get; set; }
//    public string? BuyerName { get; set; }
//    public string? BuyerMobile { get; set; }
//    public string? SellerName { get; set; }
//    public string? SellerMobile { get; set; }
//    public string? ContractDoc { get; set; }
//    public string Status { get; set; } = string.Empty;
//}

//public class EditContractDetailCommandHandler : IRequestHandler<EditContractDetailCommand, Result<int>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;

//    public EditContractDetailCommandHandler(IApplicationDbContext applicationDbContext, IJwtService jwtService)
//    {
//        _context = applicationDbContext;
//        _jwtService = jwtService;
//    }
//    public async Task<Result<int>> Handle(EditContractDetailCommand request, CancellationToken cancellationToken)
//    {
//        int userid = _jwtService.GetUserId().ToInt();

//        var entity = await _context.ContractDetails
//            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserDetailId == userid, cancellationToken);

//        if (entity == null)
//        {
//            return Result<int>.Failure(StatusCodes.Status404NotFound, AppMessages.ContractNotFound);
//        }

//        // Capture the old data snapshot as JSON
//        var oldData = new
//        {
//            entity.Id,
//            entity.Role,
//            entity.ContractTitle,
//            entity.ServiceType,
//            entity.ServiceDescription,
//            entity.AdditionalNote,
//            entity.FeesPaidBy,
//            entity.FeeAmount,
//            entity.BuyerName,
//            entity.BuyerMobile,
//            entity.SellerName,
//            entity.SellerMobile,
//            entity.ContractDoc,
//            entity.Status,
//            entity.BuyerDetailsId,
//            entity.SellerDetailsId
//        };

//        // Update entity
//        entity.Role = request.Role;
//        entity.ContractTitle = request.ContractTitle;
//        entity.ServiceType = request.ServiceType;
//        entity.ServiceDescription = request.ServiceDescription;
//        entity.AdditionalNote = request.AdditionalNote;
//        entity.FeesPaidBy = request.FeesPaidBy;
//        entity.FeeAmount = request.FeeAmount;
//        entity.BuyerName = request.BuyerName;
//        entity.BuyerMobile = request.BuyerMobile;
//        entity.SellerMobile = request.SellerMobile;
//        entity.SellerName = request.SellerName;
//        entity.Status = request.Status;
//        entity.ContractDoc = request.ContractDoc;
//        entity.BuyerDetailsId = request.Role == EscrowApIConstant.ContratConstant.ContractRoleBuyer ? userid : null;
//        entity.SellerDetailsId = request.Role == EscrowApIConstant.ContratConstant.ContractRoleSeller ? userid : null;

//        // Capture the new data snapshot as JSON
//        var newData = new
//        {
//            entity.Id,
//            entity.Role,
//            entity.ContractTitle,
//            entity.ServiceType,
//            entity.ServiceDescription,
//            entity.AdditionalNote,
//            entity.FeesPaidBy,
//            entity.FeeAmount,
//            entity.BuyerName,
//            entity.BuyerMobile,
//            entity.SellerName,
//            entity.SellerMobile,
//            entity.ContractDoc,
//            entity.Status,
//            entity.BuyerDetailsId,
//            entity.SellerDetailsId
//        };

//        // Create the log entry
//        var log = new ContractDetailsLog
//        {
//            ContractId = entity.Id,
//            Operation = "UPDATE",
//            ChangedFields = "ALL", // Optional: You can compare and list changed fields here if needed
//            PreviousData = System.Text.Json.JsonSerializer.Serialize(oldData),
//            NewData = System.Text.Json.JsonSerializer.Serialize(newData),
//            Remarks = "Full update from Edit API",
//            Created = DateTime.UtcNow,
//            ChangedBy = userid.ToString(),
//            Source = "API"
//        };

//        await _context.ContractDetailsLogs.AddAsync(log, cancellationToken);
//        _context.ContractDetails.Update(entity);
//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<int>.Success(StatusCodes.Status200OK, AppMessages.Success, entity.Id);
//    }

//}
