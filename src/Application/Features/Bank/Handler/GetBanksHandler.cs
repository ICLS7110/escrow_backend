namespace Escrow.Api.Application.Features.Handler;

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Features.Queries;

public class GetBanksHandler : IRequestHandler<GetBanksQuery, Result<PaginatedList<BankDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBanksHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<BankDto>>> Handle(GetBanksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BankDetails.AsNoTracking().ProjectTo<BankDto>(_mapper.ConfigurationProvider);
        int totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
        var paginatedResult = new PaginatedList<BankDto>(items, totalCount, request.PageNumber, request.PageSize);

        return Result<PaginatedList<BankDto>>.Success(200, "Banks retrieved successfully", paginatedResult);
    }
}
