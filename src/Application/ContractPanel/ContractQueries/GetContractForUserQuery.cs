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
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;

    public GetContractForUserQueryHandler(IApplicationDbContext context, IMapper mapper, IJwtService jwtService)
    {
        _context = context;
        _mapper = mapper;
        _jwtService = jwtService;
    }

    public async Task<PaginatedList<ContractDTO>> Handle(GetContractForUserQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        List<int> allContractIds = new();

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

            allContractIds = directContractIds.Concat(invitedContractIds).Distinct().ToList();
        }

        var query = _context.ContractDetails.AsQueryable();

        if (request.Id.HasValue && allContractIds.Any())
        {
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

        query = query.OrderByDescending(c => c.Created);
        var totalCount = await query.CountAsync(cancellationToken);

        var contracts = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var contractIds = contracts.Select(c => c.Id).ToList();

        var milestones = await _context.MileStones
            .Where(m => contractIds.Contains(Convert.ToInt32(m.ContractId)))
            .ToListAsync(cancellationToken);

        var invitations = await _context.SellerBuyerInvitations
            .Where(inv => contractIds.Contains(inv.ContractId))
            .ToListAsync(cancellationToken);

        var buyerSellerIds = contracts
            .SelectMany(c => new[] { c.BuyerDetailsId, c.SellerDetailsId })
            .Distinct()
            .ToList();

        var userProfiles = await _context.UserDetails
            .Where(u => buyerSellerIds.Contains(u.Id))
            .Select(u => new { u.Id, u.ProfilePicture })
            .ToListAsync(cancellationToken);

        Dictionary<int, string> profilePicDict = userProfiles
     .ToDictionary(u => u.Id, u => u.ProfilePicture ?? string.Empty); // or any default string


        var contractDTOs = contracts.Select(c => new ContractDTO
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
            CreatedBy = c.CreatedBy,
            Status = c.Status,
            IsActive = c.IsActive,
            IsDeleted = c.IsDeleted,
            TaxAmount = c.TaxAmount,
            EscrowTax = c.EscrowTax,
            CountryCode = PhoneNumberHelper.ExtractCountryCode(c.BuyerMobile),
            Created = c.Created,
            ContractDoc = c.ContractDoc,
            BuyerId = c.BuyerDetailsId.ToString(),
            SellerId = c.SellerDetailsId.ToString(),
            LastModified = c.LastModified,
            LastModifiedBy = c.LastModifiedBy,
            SellerPayableAmount = c.SellerPayableAmount,
            BuyerPayableAmount = c.BuyerPayableAmount,
            BuyerProfilePicture = c.BuyerDetailsId.HasValue 
    ? profilePicDict.GetValueOrDefault<int, string>(c.BuyerDetailsId.Value) 
    : null,

SellerProfilePicture = c.SellerDetailsId.HasValue 
    ? profilePicDict.GetValueOrDefault<int, string>(c.SellerDetailsId.Value) 
    : null,


            MileStones = milestones
                .Where(m => m.ContractId == c.Id)
                .Select(m => new MileStoneDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    DueDate = m.DueDate,
                    Amount = m.Amount,
                    Description = m.Description,
                    Documents = m.Documents,
                    ContractId = m.ContractId,
                    Status = m.Status
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

        return new PaginatedList<ContractDTO>(contractDTOs, totalCount, pageNumber, pageSize);
    }
}
