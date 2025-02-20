using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.UserPanel.Queries.GetUsers;

public record GetUserDetailsQuery : IRequest<PaginatedList<UserDetailDto>>
{
    public int? Id { get; init; }
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}

public class GetGetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, PaginatedList<UserDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetGetUserDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<UserDetailDto>> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        var query = _context.UserDetails.AsQueryable();

        if (request.Id.HasValue)
        {
            query = query.Where(x => x.Id == request.Id.Value);
        }

        return await query
            .OrderBy(x => x.FullName)
            .ProjectTo<UserDetailDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(pageNumber, pageSize);
    }
}
