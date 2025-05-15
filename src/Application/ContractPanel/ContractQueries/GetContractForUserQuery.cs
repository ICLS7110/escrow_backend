using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.BankDetails.Queries;
using Escrow.Api.Application.Common.Helpers;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.BankDtos;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.UserPanel.Queries.GetUsers;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Application.ContractPanel.ContractQueries;

public record GetContractForUserQuery : IRequest<PaginatedList<ContractDTO>>
{
    public int? Id { get; init; }
    public int? ContractId { get; init; }
    [FromQuery]
    public ContractStatus? Status { get; init; }  // Filter by Contract Status
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}

public class GetContractForUserQueryHandler : IRequestHandler<GetContractForUserQuery, PaginatedList<ContractDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public GetContractForUserQueryHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<PaginatedList<ContractDTO>> Handle(GetContractForUserQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        IQueryable<ContractDetails> query = _context.ContractDetails
            .AsQueryable()
            .Include(c => c.MileStones) // Include related MileStones
            .Include(c => c.BuyerDetails) // Include Buyer details
            .Include(c => c.SellerDetails) // Include Seller details
            .Include(c => c.UserDetail); // Include UserDetails if needed

        // Apply filters
        if (request.Id.HasValue)
        {
            var directContractIds = await _context.ContractDetails
                .Where(c => c.UserDetailId == request.Id || c.Id == request.ContractId)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            var invitedContractIds = await _context.SellerBuyerInvitations
                .Where(inv => inv.BuyerId == request.Id || inv.SellerId == request.Id)
                .Select(inv => inv.ContractId)
                .ToListAsync(cancellationToken);

            var allContractIds = directContractIds.Concat(invitedContractIds).Distinct().ToList();

            query = query.Where(c => allContractIds.Contains(c.Id));
        }

        if (request.ContractId.HasValue)
        {
            query = query.Where(c => c.Id == request.ContractId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.ToString());
        }

        // Pagination logic
        query = query.OrderByDescending(c => c.Created)
                     .Skip((pageNumber - 1) * pageSize)
                     .Take(pageSize);

        // Execute the query and get the results
        var contracts = await query.ToListAsync(cancellationToken);

        // Gather relevant data for contract DTOs
        var buyerSellerIds = contracts
            .SelectMany(c => new[] { c.BuyerDetailsId, c.SellerDetailsId })
            .Distinct()
            .ToList();

        var userProfiles = await _context.UserDetails
            .Where(u => buyerSellerIds.Contains(u.Id))
            .Select(u => new { u.Id, u.ProfilePicture })
            .ToListAsync(cancellationToken);

        var profilePicDict = userProfiles
            .ToDictionary(u => u.Id, u => u.ProfilePicture ?? string.Empty); // Default empty string for missing profiles

        // Map contract details into ContractDetailsDTO
        var contractDetailsDTOs = contracts.Select(c => new ContractDTO
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
            BuyerMobile = PhoneNumberHelper.ExtractPhoneNumberWithoutCountryCode(c.BuyerMobile),
            SellerName = c.SellerName,
            SellerMobile = PhoneNumberHelper.ExtractPhoneNumberWithoutCountryCode(c.SellerMobile),
            Status = c.Status,
            BuyerPayableAmount = c.BuyerPayableAmount,
            SellerPayableAmount = c.SellerPayableAmount,
            TaxAmount = c.TaxAmount,
            EscrowTax = c.EscrowTax,
            IsActive = c.IsActive,
            IsDeleted = c.IsDeleted,
            BuyerProfilePicture = profilePicDict.GetValueOrDefault(c.BuyerDetailsId ?? 0),
            SellerProfilePicture = profilePicDict.GetValueOrDefault(c.SellerDetailsId ?? 0),
            MileStones = (c.MileStones ?? new List<MileStone>())
                .Select(m => new MileStoneDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    Amount = m.Amount,
                    Description = m.Description,
                    DueDate = m.DueDate,
                    Status = m.Status
                }).ToList(),
            // BuyerDetails and SellerDetails mapping
        }).ToList();

        var totalCount = await query.CountAsync(cancellationToken);

        return new PaginatedList<ContractDTO>(contractDetailsDTOs, totalCount, pageNumber, pageSize);
    }
}
