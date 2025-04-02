using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.ContractPanel;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Domain.Enums;

public class GetContractsQuery : IRequest<Result<PaginatedList<ContractDetailsDTO>>>
{
    public int? UserId { get; set; }
    public string? Status { get; set; }
    public string? SearchKeyword { get; set; }
    public bool? IsActive { get; set; } // New property for Active/Inactive filter
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetContractsQueryHandler : IRequestHandler<GetContractsQuery, Result<PaginatedList<ContractDetailsDTO>>>
{
    private readonly IApplicationDbContext _context;

    public GetContractsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<ContractDetailsDTO>>> Handle(GetContractsQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber;
        int pageSize = request.PageSize;
        var userId = request.UserId;

        // Define allowed statuses based on IsActive filter
        List<string> activeStatuses = new() { nameof(ContractStatus.Accepted) };
        List<string> inactiveStatuses = new() { nameof(ContractStatus.Rejected), nameof(ContractStatus.Expired) };

        // Fetch contract IDs strictly from SellerBuyerInvitations table
        var invitedContractIds = await _context.SellerBuyerInvitations
            .Where(inv => inv.BuyerId == userId || inv.SellerId == userId || inv.CreatedBy == userId.ToString())
            .Select(inv => inv.ContractId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!invitedContractIds.Any())
            return Result<PaginatedList<ContractDetailsDTO>>.Failure(StatusCodes.Status404NotFound, "No contracts found.");

        // Query contracts with the specified filters
        var query = _context.ContractDetails
            .Where(c => invitedContractIds.Contains(c.Id) &&
                        (string.IsNullOrEmpty(request.Status) || c.Status == request.Status) &&
                        (string.IsNullOrEmpty(request.SearchKeyword) || c.ContractTitle.Contains(request.SearchKeyword)) &&
                        (request.IsActive == null ||
                         (request.IsActive == true && activeStatuses.Contains(c.Status)) ||
                         (request.IsActive == false && inactiveStatuses.Contains(c.Status))))
            .OrderByDescending(c => c.Created);

        var totalCount = await query.CountAsync(cancellationToken);

        var contracts = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Fetch related data: Milestones & Invitations
        var contractIdsList = contracts.Select(c => c.Id).ToList();

        var milestones = await _context.MileStones
            .Where(m => contractIdsList.Contains(Convert.ToInt32(m.ContractId)))
            .ToListAsync(cancellationToken);

        var invitations = await _context.SellerBuyerInvitations
            .Where(inv => contractIdsList.Contains(inv.ContractId))
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var contractDTOs = contracts.Select(c => new ContractDetailsDTO
        {
            Id = c.Id,
            Role = c.Role ?? string.Empty,
            ContractTitle = c.ContractTitle,
            ServiceType = c.ServiceType,
            ServiceDescription = c.ServiceDescription,
            AdditionalNote = c.AdditionalNote,
            FeesPaidBy = c.FeesPaidBy,
            FeeAmount = c.FeeAmount,
            BuyerName = c.BuyerName,
            BuyerMobile = c.BuyerMobile,
            SellerName = c.SellerName,
            SellerMobile = c.SellerMobile,
            CreatedBy = c.CreatedBy,
            Status = c.Status,
            IsActive = c.IsActive,
            IsDeleted = c.IsDeleted,
            TaxAmount = c.TaxAmount,
            EscrowTax = c.EscrowTax,
            Created = c.Created,
            ContractDoc = c.ContractDoc,
            LastModified = c.LastModified,

            MileStones = milestones
                .Where(m => m.ContractId == c.Id)
                .Select(m => new MileStoneDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Amount = m.Amount,
                    DueDate = m.DueDate,
                }).ToList(),

            InvitationDetails = invitations
                .Where(inv => inv.ContractId == c.Id)
                .Select(inv => new SellerBuyerInvitation
                {
                    Id = inv.Id,
                    ContractId = inv.ContractId,
                    BuyerId = inv.BuyerId,
                    SellerId = inv.SellerId,
                    Status = inv.Status,
                    Created = inv.Created
                }).FirstOrDefault()
        }).ToList();

        var paginatedList = new PaginatedList<ContractDetailsDTO>(contractDTOs, totalCount, pageNumber, pageSize);

        return Result<PaginatedList<ContractDetailsDTO>>.Success(StatusCodes.Status200OK, "Contracts retrieved successfully.", paginatedList);
    }
}





























//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models.ContractDTOs;
//using Escrow.Api.Domain.Entities.ContractPanel;
//using MediatR;
//using Microsoft.EntityFrameworkCore;

//namespace Escrow.Api.Application.ContractPanel.ContractQueries;

//public class GetContractsQuery : IRequest<List<ContractDetailsDTO>>
//{
//    public int? UserId { get; set; }
//    public string? status { get; set; }
//}

//public class GetContractsQueryHandler : IRequestHandler<GetContractsQuery, List<ContractDetailsDTO>>
//{
//    private readonly IApplicationDbContext _context;

//    public GetContractsQueryHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<List<ContractDetailsDTO>> Handle(GetContractsQuery request, CancellationToken cancellationToken)
//    {
//        // Step 1: Get contract IDs where the user is involved (directly or via invitation)
//        var directContractIds = await _context.ContractDetails
//            .Where(c => c.UserDetailId == request.UserId || c.Id == request.UserId)
//            .Select(c => c.Id)
//            .ToListAsync(cancellationToken);

//        var invitedContractIds = await _context.SellerBuyerInvitations
//            .Where(inv => inv.BuyerId == request.UserId || inv.SellerId == request.UserId)
//            .Select(inv => inv.ContractId)
//            .ToListAsync(cancellationToken);

//        var allContractIds = directContractIds.Concat(invitedContractIds).Distinct().ToList();

//        if (!allContractIds.Any())
//            return new List<ContractDetailsDTO>();

//        // Step 2: Fetch all contracts based on the collected contract IDs
//        var contracts = await _context.ContractDetails
//            .Where(c => allContractIds.Contains(c.Id))
//            .ToListAsync(cancellationToken);

//        // Step 3: Fetch milestones related to the contracts
//        var milestones = await _context.MileStones
//            .Where(m => allContractIds.Contains(Convert.ToInt32(m.ContractId)))
//            .ToListAsync(cancellationToken);

//        // Step 4: Fetch invitation details for contracts
//        var invitations = await _context.SellerBuyerInvitations
//            .Where(inv => allContractIds.Contains(inv.ContractId))
//            .ToListAsync(cancellationToken);

//        // Step 5: Map the contracts with their respective milestones and invitations
//        var contractDTOs = contracts.Select(c => new ContractDetailsDTO
//        {
//            Id = c.Id,
//            ContractTitle = c.ContractTitle,
//            ServiceType = c.ServiceType,
//            ServiceDescription = c.ServiceDescription,
//            AdditionalNote = c.AdditionalNote,
//            FeesPaidBy = c.FeesPaidBy,
//            FeeAmount = c.FeeAmount,
//            BuyerName = c.BuyerName,
//            BuyerMobile = c.BuyerMobile,
//            SellerName = c.SellerName,
//            SellerMobile = c.SellerMobile,
//            CreatedBy = c.CreatedBy,
//            Status = c.Status,
//            IsActive = c.IsActive,
//            IsDeleted = c.IsDeleted,
//            TaxAmount = c.TaxAmount,
//            EscrowTax = c.EscrowTax,
//            Created = c.Created,

//            // Add milestones to the contract
//            MileStones = milestones
//                .Where(m => m.ContractId == c.Id)
//                .Select(m => new MileStoneDTO
//                {
//                    Id = m.Id,
//                    Name = m.Name,
//                    Description = m.Description,
//                    Amount = m.Amount,
//                    DueDate = m.DueDate,
//                })
//                .ToList(),

//            // Add invitation details to the contract
//            InvitationDetails = invitations
//                .Where(inv => inv.ContractId == c.Id)
//                .Select(inv => new SellerBuyerInvitation
//                {
//                    Id = inv.Id,
//                    ContractId = inv.ContractId,
//                    BuyerId = inv.BuyerId,
//                    SellerId = inv.SellerId,
//                    Status = inv.Status,
//                    Created = inv.Created
//                })
//                .FirstOrDefault() // One contract has one invitation
//        }).ToList();

//        return contractDTOs;
//    }
//}
