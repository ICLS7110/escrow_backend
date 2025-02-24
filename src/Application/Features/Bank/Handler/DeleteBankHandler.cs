namespace Escrow.Api.Application.Handler; 

using System;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Features.Commands;
using Escrow.Api.Domain.Enums;

public class DeleteBankHandler : IRequestHandler<DeleteBankCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    public DeleteBankHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<string>> Handle(DeleteBankCommand request, CancellationToken cancellationToken)
    {
        int userId = int.Parse(_jwtService.GetUserId());

        var entity = await _context.BankDetails
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserDetailId == userId, cancellationToken);

        if (entity == null)
        {
            return Result<string>.Failure(404, "Bank details not found.");
        }

        entity.RecordState = RecordState.Deleted;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = userId;

        await _context.SaveChangesAsync(cancellationToken); 

        return Result<string>.Success(200, "Bank details deleted successfully.");
    }
}
