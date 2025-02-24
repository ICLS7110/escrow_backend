namespace Escrow.Api.Application.Handler;

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;



public record GetUserDetailsQuery : IRequest<Result<PaginatedList<UserDto>>>
{
    public int? Id { get; init; }
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}

public class GetGetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, Result<PaginatedList<UserDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetGetUserDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<UserDto>>> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        var query = _context.UserDetails.AsQueryable();

        var result = await query
            .OrderBy(x => x.FullName)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(pageNumber, pageSize);

        return Result<PaginatedList<UserDto>>.Success(400, "Bad Request", result);
    }

}
