using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.UserPanel.Queries.GetUsers;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.BankDetails.Queries;

public record GetBankDetailsQuery : IRequest<PaginatedList<BankDetail>>
{
    public int? Id { get; init; }
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}

public class GetBankDetailsQueryHandler : IRequestHandler<GetBankDetailsQuery, PaginatedList<BankDetail>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBankDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<BankDetail>> Handle(GetBankDetailsQuery request, CancellationToken cancellationToken)
    {
        if (request.Id.HasValue)
        {
            return await _context.BankDetails
            .Where(x => x.Id == Convert.ToInt32(request.Id))
            .OrderBy(x => x.AccountHolderName)
                //.ProjectTo<BankDetail>(_mapper.ConfigurationProvider)
                .PaginatedListAsync(request.PageNumber ?? 1, request.PageSize ?? 10);
        }
        else
        {
            return await _context.BankDetails
                .OrderBy(x => x.AccountHolderName)
                //.ProjectTo<BankDetail>(_mapper.ConfigurationProvider)
                .PaginatedListAsync(request.PageNumber ?? 1, request.PageSize ?? 10);
        }
    }
}
