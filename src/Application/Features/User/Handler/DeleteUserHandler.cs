namespace Escrow.Api.Application.Handler;

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Enums;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Features.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    public DeleteUserCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<string>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.UserDetails
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            return Result<string>.Failure(404, "User Details Not Found.");
        }

        entity.RecordState = RecordState.Deleted;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = int.Parse(_jwtService.GetUserId());
        _context.UserDetails.Update(entity);           

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success(200, "Deleted.",null);
    }

}
