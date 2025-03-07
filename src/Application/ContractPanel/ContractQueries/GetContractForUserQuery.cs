using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.BankDetails.Queries;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.BankDtos;
using Escrow.Api.Application.Common.Mappings;

namespace Escrow.Api.Application.ContractPanel.ContractQueries;

public record GetContractForUserQuery : IRequest<PaginatedList<ContractDetailsDTO>>
{
    public int? Id { get; init; }
    public int? ContractId { get; init; }
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

        var query = _context.ContractDetails.AsQueryable();

        if (request.Id.HasValue)
        {
            query = query.Where(x => x.UserDetailId == request.Id.Value);
        }

        if (request.ContractId.HasValue)
        {
            query = query.Where(x => x.Id == request.ContractId.Value);
        }

        return await query
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
                    Documents=m.Documents,
                    ContractId=m.ContractId
                }).ToList()
            })
            .OrderBy(x => x.ContractTitle)
            .PaginatedListAsync(pageNumber, pageSize);
    }
}
