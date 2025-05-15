using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.Disputes.Queries;

public record GetDisputesQuery : IRequest<PaginatedList<DisputeDTO>>
{
    public int? DisputeId { get; init; } // Optional
    public DisputeStatus? Status { get; init; }  // Optional
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetDisputesQueryHandler : IRequestHandler<GetDisputesQuery, PaginatedList<DisputeDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public GetDisputesQueryHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }



    public async Task<PaginatedList<DisputeDTO>> Handle(GetDisputesQuery request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.GetUserId().ToInt();
        var user = await _context.UserDetails.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        var query = _context.Disputes.AsQueryable();

        if (request.DisputeId.HasValue)
        {
            query = query.Where(d => d.Id == request.DisputeId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(d => d.Status == nameof(request.Status));
        }

        if (user?.Role == nameof(Roles.User))
        {
            query = query.Where(d => d.DisputeRaisedBy == userId.ToString()); // Assuming you track who raised the dispute
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        var disputes = await query
            .OrderByDescending(d => d.DisputeDateTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Fetch contract details based on contract IDs in the disputes
        var contractIds = disputes.Select(d => d.ContractId).Distinct().ToList();

        var contracts = await _context.ContractDetails
            .Where(c => contractIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var disputeDTOs = disputes.Select(d =>
        {
            var contract = contracts.TryGetValue(d.ContractId, out var c) ? c : null;

            return new DisputeDTO
            {
                Id = d.Id,
                DisputeDateTime = d.DisputeDateTime,
                RaisedBy = d.DisputeRaisedBy ?? "Unknown",
                DisputeDoc = d.DisputeDoc ?? "Unknown",
                Status = d.Status?.ToString() ?? "N/A",
                DisputeDescription = d.DisputeDescription?.ToString() ?? "N/A",
                DisputeReason = d.DisputeReason?.ToString() ?? "N/A",

                ContractDetails = contract != null ? new ContractDTO
                {
                    Id = contract.Id,
                    ContractTitle = contract.ContractTitle,
                    ServiceType = contract.ServiceType,
                    ServiceDescription = contract.ServiceDescription,
                    AdditionalNote = contract.AdditionalNote,
                    FeesPaidBy = contract.FeesPaidBy,
                    FeeAmount = contract.FeeAmount,
                    BuyerId = contract.BuyerDetailsId.ToString(),
                    BuyerName = contract.BuyerName,
                    BuyerMobile = contract.BuyerMobile,
                    SellerId = contract.SellerDetailsId.ToString(),
                    SellerName = contract.SellerName,
                    SellerMobile = contract.SellerMobile,
                    Status = contract.Status,
                    IsActive = contract.IsActive,
                    IsDeleted = contract.IsDeleted,
                    TaxAmount = contract.TaxAmount,
                    EscrowTax = contract.EscrowTax,
                    CreatedBy = contract.CreatedBy,
                    LastModifiedBy = contract.LastModifiedBy,
                    BuyerPayableAmount = contract.BuyerPayableAmount,
                    SellerPayableAmount = contract.SellerPayableAmount,
                    Created = contract.Created,
                    LastModified = contract.LastModified,
                    // Leave collections empty for now unless you need to fill them:
                    MileStones = new List<MileStoneDTO>(),
                    InvitationDetails = null,
                    TeamMembers = new List<TeamDTO>()
                } : null
            };
        }).ToList();


        return new PaginatedList<DisputeDTO>(disputeDTOs, totalRecords, request.PageNumber, request.PageSize);
    }
}
