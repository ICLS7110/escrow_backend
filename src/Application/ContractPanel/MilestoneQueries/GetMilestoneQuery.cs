using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Models.BankDtos;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.BankDetails.Queries;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.Common.Mappings;

namespace Escrow.Api.Application.ContractPanel.MilestoneQueries;

public record GetMilestoneQuery : IRequest<PaginatedList<MileStoneDTO>>
{
    public int? Id { get; set; }
    public int? ContractId { get; init; }
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}

public class GetMilestoneQueryHandler : IRequestHandler<GetMilestoneQuery, PaginatedList<MileStoneDTO>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;

    public GetMilestoneQueryHandler(IApplicationDbContext context, IMapper mapper, IJwtService jwtService)
    {
        _context = context;
        _mapper = mapper;
        _jwtService = jwtService;
    }

    public async Task<PaginatedList<MileStoneDTO>> Handle(GetMilestoneQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        var query = _context.MileStones.AsQueryable();

        if (request.ContractId.HasValue)
        {
            query = query.Where(x => x.ContractId == request.ContractId.Value);
        }

        return await query
            .Select(s => new MileStoneDTO
            {
                Id = s.Id,
                Name = s.Name,
                Amount = s.Amount,
                Description = s.Description,
                DueDate = s.DueDate,
                Documents = s.Documents,
                ContractId = s.ContractId,
                Status = s.Status
            })
            .OrderBy(x => x.Name)
            .PaginatedListAsync(pageNumber, pageSize);
    }
}
