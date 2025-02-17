using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.BankDetails.Commands;

public record DeleteBankDetailCommand(int Id): IRequest;

public class DeleteBankDetailCommandHandler : IRequestHandler<DeleteBankDetailCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    public DeleteBankDetailCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task Handle(DeleteBankDetailCommand request, CancellationToken cancellationToken)
    {
        var userid= _jwtService.GetUserId().ToInt();
        var entity = await _context.BankDetails
            .FirstOrDefaultAsync(x => x.Id==request.Id && x.UserDetailId==userid);

        if (entity == null)
        {
            throw new EscrowDataNotFoundException("Bank Details Not Found.");
        }
        entity.RecordState = RecordState.Deleted;
        entity.DeletedAt= DateTimeOffset.UtcNow;
        entity.DeletedBy = userid;
        _context.BankDetails.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }  
}
