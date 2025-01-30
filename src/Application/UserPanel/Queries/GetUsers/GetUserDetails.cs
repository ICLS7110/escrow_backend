using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.UserPanel.Queries.GetUsers;

public record GetUserDetailsQuery : IRequest<List<UserDetail>>
{
    public int? Id{ get; init; }
    /* public int PageNumber { get; init; } = 1;
     public int PageSize { get; init; } = 10;*/
}

public class GetGetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, List<UserDetail>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetGetUserDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<UserDetail>> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        if (request.Id.HasValue)
        {
            return await _context.UserDetails
            .Where(x => x.Id == Convert.ToInt32(request.Id)).ToListAsync(cancellationToken);            
        }
        else
        {
            return await _context.UserDetails.ToListAsync(cancellationToken);
        }
        
        //.OrderBy(x => x.FullName)
        //.ProjectTo<UserDetailDto>(_mapper.ConfigurationProvider);
        //.PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
