namespace Escrow.Api.Application.Handler;

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Features.Queries;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserByIdHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        UserDto? result = null;

        var query = _context.UserDetails.AsQueryable();

        if (int.TryParse(request.Id, out int id))
            query = query.Where(x => x.Id == id);
        else
            return Result<UserDto>.Failure(400, "Bad Requeest");

        result = await query.ProjectTo<UserDto>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

        return Result<UserDto>.Success(400, "Bad Requeest", result);
    }
}
