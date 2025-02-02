using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;

namespace Escrow.Api.Application.BankDetails.Commands;

public record DeleteBankDetailCommand(int Id): IRequest;

public class DeleteBankDetailCommandHandler : IRequestHandler<DeleteBankDetailCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteBankDetailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteBankDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.BankDetails
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.BankDetails.Remove(entity);

        //entity.AddDomainEvent(new BankDetailDeletedEvent(entity));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
