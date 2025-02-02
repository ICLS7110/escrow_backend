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
        if (request.Id.HasValue)
        {
            return await _context.UserDetails
            .Where(x => x.Id == Convert.ToInt32(request.Id))
            .OrderBy(x => x.FullName)
                .ProjectTo<UserDetailDto>(_mapper.ConfigurationProvider)
                .PaginatedListAsync(request.PageNumber ?? 1, request.PageSize ?? 10);
        }
        else
        {
            return await _context.UserDetails
                .OrderBy(x => x.FullName)
                .ProjectTo<UserDetailDto>(_mapper.ConfigurationProvider)
                .PaginatedListAsync(request.PageNumber ?? 1, request.PageSize ?? 10);
        }
    }
}
