using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.ContractPanel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Escrow.Api.Application.ContractPanel.ContractCommands;
//public class ModifyContractCommand : IRequest<Result<string>>
//{

//    public int? ContractId { get; set; }
//    public string? Role { get; set; }
//    public string? ContractTitle { get; set; }
//    public string? ServiceType { get; set; }
//    public string? ServiceDescription { get; set; }
//    public string? AdditionalNote { get; set; }
//    public string? FeesPaidBy { get; set; }
//    public decimal FeeAmount { get; set; }
//    public string? BuyerName { get; set; }
//    public string? BuyerMobile { get; set; }
//    public string? SellerName { get; set; }
//    public string? SellerMobile { get; set; }
//    public string? ContractDoc { get; set; }
//    public string? Status { get; set; }
//    public int InvitationId { get; set; }
//    public List<MileStoneDTO>? MileStoneDetails { get; set; }
//}

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

    public ModifyContractCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<object>> Handle(ModifyContractCommand request, CancellationToken cancellationToken)
    {
        int userId = _jwtService.GetUserId().ToInt();

        // Retrieve and update contract details
        var contract = await _context.ContractDetails
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

        if (contract == null)
            return Result<object>.Failure(StatusCodes.Status404NotFound, "Contract not found.");

        var oldData = JsonConvert.SerializeObject(contract);

        // Update contract properties
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
        contract.Status = request.Status ?? string.Empty;
        contract.LastModified = DateTime.UtcNow;
        contract.LastModifiedBy = userId.ToString();

        // Handle milestone updates
        var existingMilestones = await _context.MileStones
            .Where(x => x.ContractId == contract.Id)
            .ToListAsync(cancellationToken);

        var requestMilestones = request.MileStoneDetails ?? new List<MileStoneDTO>();

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
                existing.Status = dto.Status;
                existing.MileStoneEscrowAmount = dto.MileStoneEscrowAmount;
                existing.MileStoneTaxAmount = dto.MileStoneTaxAmount;

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
                    Status = dto.Status,
                    MileStoneEscrowAmount = dto.MileStoneEscrowAmount,
                    MileStoneTaxAmount = dto.MileStoneTaxAmount,
                    Created = DateTime.UtcNow,
                    CreatedBy = userId.ToString()
                };

                await _context.MileStones.AddAsync(newMilestone, cancellationToken);
            }
        }

        // Update SellerBuyerInvitation if InvitationId is provided
        if (request.InvitationId > 0)
        {
            var invitation = await _context.SellerBuyerInvitations
                .FirstOrDefaultAsync(i => i.ContractId == contract.Id && i.Id == request.InvitationId, cancellationToken);

            if (invitation != null)
            {
                invitation.Status = "Updated"; // Example: Update status or other fields as necessary
                invitation.SellerPhoneNumber = request.SellerMobile ?? invitation.SellerPhoneNumber;
                invitation.BuyerPhoneNumber = request.BuyerMobile ?? invitation.BuyerPhoneNumber;

                // Update other properties if necessary
                _context.SellerBuyerInvitations.Update(invitation);
            }
            else
            {
                // You can handle this case if no invitation exists
                return Result<object>.Failure(StatusCodes.Status404NotFound, "Invitation not found.");
            }
        }

        // Save all changes
        await _context.SaveChangesAsync(cancellationToken);

        var newData = JsonConvert.SerializeObject(contract);

        // Save log
        var log = new ContractDetailsLog
        {
            ContractId = contract.Id,
            Operation = "UPDATE",
            ChangedFields = "All", // You can add diff logic if needed
            PreviousData = oldData,
            NewData = newData,
            Remarks = "Contract updated by user.",
            Created = DateTime.UtcNow,
            ChangedBy = userId.ToString(),
            Source = "API"
        };

        await _context.ContractDetailsLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object>.Success(StatusCodes.Status200OK, "Contract updated successfully.", new { contract.Id });
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
//        int userId = _jwtService.GetUserId().ToInt();

//        var contract = await _context.ContractDetails
//            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

//        if (contract == null)
//            return Result<object>.Failure(StatusCodes.Status404NotFound, "Contract not found.");

//        var oldData = JsonConvert.SerializeObject(contract);

//        // Update contract properties
//        contract.Role = request.Role ?? string.Empty;
//        contract.ContractTitle = request.ContractTitle ?? string.Empty;
//        contract.ServiceType = request.ServiceType ?? string.Empty;
//        contract.ServiceDescription = request.ServiceDescription ?? string.Empty;
//        contract.AdditionalNote = request.AdditionalNote ?? string.Empty;
//        contract.FeesPaidBy = request.FeesPaidBy ?? string.Empty;
//        contract.FeeAmount = request.FeeAmount;
//        contract.BuyerName = request.BuyerName ?? string.Empty;
//        contract.BuyerMobile = request.BuyerMobile ?? string.Empty;
//        contract.SellerName = request.SellerName ?? string.Empty;
//        contract.SellerMobile = request.SellerMobile ?? string.Empty;
//        contract.ContractDoc = request.ContractDoc ?? string.Empty;
//        contract.Status = request.Status ?? string.Empty;
//        contract.LastModified = DateTime.UtcNow;
//        contract.LastModifiedBy = userId.ToString();

//        // Handle milestone updates
//        var existingMilestones = await _context.MileStones
//            .Where(x => x.ContractId == contract.Id)
//            .ToListAsync(cancellationToken);

//        var requestMilestones = request.MileStoneDetails ?? new List<MileStoneDTO>();

//        foreach (var dto in requestMilestones)
//        {
//            var existing = existingMilestones.FirstOrDefault(x => x.Id == dto.Id);

//            if (existing != null)
//            {
//                existing.Name = dto.Name;
//                existing.Amount = dto.Amount;
//                existing.Description = dto.Description;
//                existing.DueDate = dto.DueDate;
//                existing.Documents = dto.Documents;
//                existing.Status = dto.Status;
//                existing.MileStoneEscrowAmount = dto.MileStoneEscrowAmount;
//                existing.MileStoneTaxAmount = dto.MileStoneTaxAmount;

//                _context.MileStones.Update(existing);
//            }
//            else
//            {
//                var newMilestone = new MileStone
//                {
//                    ContractId = contract.Id,
//                    Name = dto.Name,
//                    Amount = dto.Amount,
//                    Description = dto.Description,
//                    DueDate = dto.DueDate,
//                    Documents = dto.Documents,
//                    Status = dto.Status,
//                    MileStoneEscrowAmount = dto.MileStoneEscrowAmount,
//                    MileStoneTaxAmount = dto.MileStoneTaxAmount,
//                    Created = DateTime.UtcNow,
//                    CreatedBy = userId.ToString()
//                };

//                await _context.MileStones.AddAsync(newMilestone, cancellationToken);
//            }
//        }

//        await _context.SaveChangesAsync(cancellationToken);

//        var newData = JsonConvert.SerializeObject(contract);

//        // Save log
//        var log = new ContractDetailsLog
//        {
//            ContractId = contract.Id,
//            Operation = "Modify",
//            ChangedFields = "All", // You can add diff logic if needed
//            PreviousData = oldData,
//            NewData = newData,
//            Remarks = "Contract updated by user.",
//            Created = DateTime.UtcNow,
//            ChangedBy = userId.ToString(),
//            Source = "API"
//        };

//        await _context.ContractDetailsLogs.AddAsync(log, cancellationToken);
//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<object>.Success(StatusCodes.Status200OK, "Contract updated successfully.", new { contract.Id });
//    }
//}

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
//        int userId = _jwtService.GetUserId().ToInt();

//        var contract = await _context.ContractDetails
//            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

//        if (contract == null)
//            return Result<object>.Failure(StatusCodes.Status404NotFound, "Contract not found.");

//        var oldData = JsonConvert.SerializeObject(contract);
//        // Update contract details
//        contract.Role = request.Role ?? string.Empty;
//        contract.ContractTitle = request.ContractTitle ?? string.Empty;
//        contract.ServiceType = request.ServiceType ?? string.Empty;
//        contract.ServiceDescription = request.ServiceDescription ?? string.Empty;
//        contract.AdditionalNote = request.AdditionalNote ?? string.Empty;
//        contract.FeesPaidBy = request.FeesPaidBy ?? string.Empty;
//        contract.FeeAmount = request.FeeAmount;
//        contract.BuyerName = request.BuyerName ?? string.Empty;
//        contract.BuyerMobile = request.BuyerMobile ?? string.Empty;
//        contract.SellerName = request.SellerName ?? string.Empty;
//        contract.SellerMobile = request.SellerMobile ?? string.Empty;
//        contract.ContractDoc = request.ContractDoc ?? string.Empty;
//        contract.Status = request.Status ?? string.Empty;
//        contract.LastModified = DateTime.UtcNow;
//        contract.LastModifiedBy = userId.ToString();


//        var existingMilestones = await _context.MileStones
//            .Where(x => x.ContractId == contract.Id)
//            .ToListAsync(cancellationToken);

//        var requestMilestones = request.MileStoneDetails ?? new List<MileStoneDTO>();

//        foreach (var dto in requestMilestones)
//        {
//            var existing = existingMilestones.FirstOrDefault(x => x.Id == dto.Id);

//            if (existing != null)
//            {
//                existing.Name = dto.Name;
//                existing.Amount = dto.Amount;
//                existing.Description = dto.Description;
//                existing.DueDate = dto.DueDate;
//                existing.Documents = dto.Documents;
//                existing.Status = dto.Status;
//                existing.MileStoneEscrowAmount = dto.MileStoneEscrowAmount;
//                existing.MileStoneTaxAmount = dto.MileStoneTaxAmount;

//                _context.MileStones.Update(existing);
//            }
//            else
//            {
//                var newMilestone = new MileStone
//                {
//                    ContractId = contract.Id,
//                    Name = dto.Name,
//                    Amount = dto.Amount,
//                    Description = dto.Description,
//                    DueDate = dto.DueDate,
//                    Documents = dto.Documents,
//                    Status = dto.Status,
//                    MileStoneEscrowAmount = dto.MileStoneEscrowAmount,
//                    MileStoneTaxAmount = dto.MileStoneTaxAmount,
//                    Created = DateTime.UtcNow,
//                    CreatedBy = userId.ToString()
//                };

//                await _context.MileStones.AddAsync(newMilestone, cancellationToken);
//            }
//        }

//        await _context.SaveChangesAsync(cancellationToken);

//        var newData = JsonConvert.SerializeObject(contract);
//        // Optionally log oldData vs newData

//        return Result<object>.Success(StatusCodes.Status200OK, "Contract updated successfully.", new { contract.Id });
//    }
//}
