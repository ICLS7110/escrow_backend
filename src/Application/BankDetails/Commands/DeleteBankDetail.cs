namespace Escrow.Api.Application.BankDetails.Commands;

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;


public record DeleteBankDetailCommand(int Id): IRequest<Result<int>>;

public class DeleteBankDetailCommandHandler : IRequestHandler<DeleteBankDetailCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    public DeleteBankDetailCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<int>> Handle(DeleteBankDetailCommand request, CancellationToken cancellationToken)
    {
        var userid= _jwtService.GetUserId().ToInt();
        var entity = await _context.BankDetails
            .FirstOrDefaultAsync(x => x.Id==request.Id && x.UserDetailId==userid);

        if (entity == null)
        {
            return Result<int>.Failure(404,"Not Fount");
        }
        entity.RecordState = RecordState.Deleted;
        entity.DeletedAt= DateTimeOffset.UtcNow;
        entity.DeletedBy = userid;
        _context.BankDetails.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(200, "Success");
    }  
}
