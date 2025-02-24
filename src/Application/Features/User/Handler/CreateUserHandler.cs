namespace Escrow.Api.Application.Handler;

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Features.Commands;
using Escrow.Api.Domain.Entities;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public CreateUserHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        bool isExists = await _context.UserDetails.AnyAsync(x => x.BusinessEmail == request.BusinessEmail, cancellationToken);
        if (isExists)
        {
            throw new ValidationException("The business email address is already registered with weLink.com");
        }

        var entity = _mapper.Map<User>(request);

        _context.UserDetails.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(entity);

        return Result<UserDto>.Success(200, "User Created", userDto);
    }

}
