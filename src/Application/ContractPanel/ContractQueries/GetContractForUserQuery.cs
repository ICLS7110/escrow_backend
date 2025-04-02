using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.BankDetails.Queries;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.BankDtos;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Application.ContractPanel.ContractQueries;

public record GetContractForUserQuery : IRequest<PaginatedList<ContractDetailsDTO>>
{
    public int? Id { get; init; }
    public int? ContractId { get; init; }
    [FromQuery]
    public ContractStatus? Status { get; init; }  // Filter by Contract Status
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}
public class GetContractForUserQueryHandler : IRequestHandler<GetContractForUserQuery, PaginatedList<ContractDetailsDTO>>
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

    public async Task<PaginatedList<ContractDetailsDTO>> Handle(GetContractForUserQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        // Step 1: Get contract IDs where the user is involved
        var directContractIds = await _context.ContractDetails
            .Where(c => c.UserDetailId == request.Id || c.Id == request.ContractId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var invitedContractIds = await _context.SellerBuyerInvitations
            .Where(inv => inv.BuyerId == request.Id || inv.SellerId == request.Id)
            .Select(inv => inv.ContractId)
            .ToListAsync(cancellationToken);

        var allContractIds = directContractIds.Concat(invitedContractIds).Distinct().ToList();

        if (!allContractIds.Any())
            return new PaginatedList<ContractDetailsDTO>(new List<ContractDetailsDTO>(), 0, pageNumber, pageSize);

        // Step 2: Query contracts with filtering
        var query = _context.ContractDetails
            .Where(c => allContractIds.Contains(c.Id) &&
                        (!request.ContractId.HasValue || c.Id == request.ContractId.Value) &&
                        (!request.Status.HasValue || c.Status == request.Status.ToString()))
            .OrderByDescending(c => c.Created);

        var totalCount = await query.CountAsync(cancellationToken);

        var contracts = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Step 3: Fetch related data
        var contractIds = contracts.Select(c => c.Id).ToList();
        var milestones = await _context.MileStones
            .Where(m => contractIds.Contains(Convert.ToInt32(m.ContractId)))
            .ToListAsync(cancellationToken);

        var invitations = await _context.SellerBuyerInvitations
            .Where(inv => contractIds.Contains(inv.ContractId))
            .ToListAsync(cancellationToken);

        // Step 4: Map contracts to DTOs
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
                    DueDate = m.DueDate,
                    Amount = m.Amount,
                    Description = m.Description,
                    Documents = m.Documents,
                    ContractId = m.ContractId
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

        return new PaginatedList<ContractDetailsDTO>(contractDTOs, totalCount, pageNumber, pageSize);
    }
}
