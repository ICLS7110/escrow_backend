namespace Escrow.Api.Application.Handler;

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Features.Commands;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    public UpdateUserHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.UserDetails
            .FindAsync(new object[] { int.Parse(_jwtService.GetUserId()) }, cancellationToken);

        if (entity == null) 
        {
            return Result<UserDto>.Failure(404, "User Details Not Found.");
        }
        
        entity.FullName = request.FullName;
        entity.EmailAddress = request.EmailAddress;
        entity.Gender = request.Gender;
        entity.DateOfBirth = request.DateOfBirth;
        entity.BusinessManagerName = request.BusinessManagerName;
        entity.BusinessEmail = request.BusinessEmail;
        entity.VatId = request.VatId;
        entity.BusinessProof = request.BusinessProof;
        entity.CompanyEmail = request.CompanyEmail;
        entity.ProfilePicture = request.ProfilePicture;
        
        await _context.SaveChangesAsync(cancellationToken);
        return Result<UserDto>.Success(200, "User Details Not Found.", new UserDto() { });
    }
}
