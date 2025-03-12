using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Mappings;

namespace Escrow.Api.Application.ContractPanel.ContractQueries
{
    public record GetContractForBuyerQuery : IRequest<PaginatedList<ContractDetailsDTO>>
    {
        public int? Id { get; init; }
        public int? BuyerId { get; init; }
        public int? SellerId { get; init; }
        public int? ContractId { get; init; }
        public int? PageNumber { get; init; } = 1;
        public int? PageSize { get; init; } = 10;
    }

    public class GetContractForBuyerQueryHandler : IRequestHandler<GetContractForBuyerQuery, PaginatedList<ContractDetailsDTO>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;

        public GetContractForBuyerQueryHandler(IApplicationDbContext context, IMapper mapper, IJwtService jwtService)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        public async Task<PaginatedList<ContractDetailsDTO>> Handle(GetContractForBuyerQuery request, CancellationToken cancellationToken)
        {
            // Default to page 1 and page size 10, ensure they are valid.
            int pageNumber = Math.Max(request.PageNumber ?? 1, 1);
            int pageSize = Math.Max(request.PageSize ?? 10, 1);

            // Start by querying the SellerBuyerInvitation to find the contract based on SellerId, BuyerId, and ContractId
            var invitationQuery = _context.SellerBuyerInvitations.AsQueryable();

            // Filter by ContractId if provided
            if (request.ContractId.HasValue)
            {
                invitationQuery = invitationQuery.Where(x => x.ContractId == request.ContractId.Value);
            }

            // Filter by BuyerId if provided
            if (request.BuyerId.HasValue)
            {
                invitationQuery = invitationQuery.Where(x => x.BuyerId == request.BuyerId.Value);
            }

            // Filter by SellerId if provided
            if (request.SellerId.HasValue)
            {
                invitationQuery = invitationQuery.Where(x => x.SellerId == request.SellerId.Value);
            }

            // Fetch all invitations that match the criteria
            var invitations = await invitationQuery.ToListAsync(cancellationToken);

            // If no invitation is found, return an empty paginated list manually
            if (!invitations.Any())
            {
                return new PaginatedList<ContractDetailsDTO>(new List<ContractDetailsDTO>(), 0, pageNumber, pageSize);
            }

            // Now, query all contracts that match the invitations
            var contractIds = invitations.Select(i => i.ContractId).Distinct().ToList();

            var query = _context.ContractDetails.AsQueryable();

            // Filter by contract ids that match the invitations
            query = query.Where(x => contractIds.Contains(x.Id));

            // Project to DTO and include milestones
            var result = await query
                .Select(s => new ContractDetailsDTO
                {
                    Id = s.Id,
                    Role = s.Role,
                    ContractTitle = s.ContractTitle,
                    ServiceType = s.ServiceType,
                    ServiceDescription = s.ServiceDescription,
                    AdditionalNote = s.AdditionalNote,
                    FeesPaidBy = s.FeesPaidBy,
                    FeeAmount = s.FeeAmount,
                    BuyerName = s.BuyerName,
                    BuyerMobile = s.BuyerMobile,
                    SellerName = s.SellerName,
                    SellerMobile = s.SellerMobile,
                    Status = s.Status,
                    MileStones = _context.MileStones
                        .Where(m => m.ContractId == s.Id)
                        .Select(m => new MileStoneDTO
                        {
                            Id = m.Id,
                            Name = m.Name,
                            DueDate = m.DueDate,
                            Amount = m.Amount,
                            Description = m.Description,
                            Documents = m.Documents,
                            ContractId = m.ContractId
                        }).ToList()
                })
                .OrderBy(x => x.ContractTitle) // Order the results by contract title
                .PaginatedListAsync(pageNumber, pageSize); // Paginate results

            return result;
        }

         
    }
}
