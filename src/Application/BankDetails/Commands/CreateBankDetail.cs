using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.BankDetails.Commands;
public record CreateBankDetailCommand : IRequest<int>
{
    public int UserDetailId { get; set; }
    public string AccountHolderName { get; set; } = string.Empty;
    public string IBANNumber { get; set; } = string.Empty;
    public string BICCode { get; set; } = string.Empty;
}

public class CreateBankDetailCommandHandler : IRequestHandler<CreateBankDetailCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateBankDetailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateBankDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = new BankDetail
        {
            UserDetailId = request.UserDetailId,
            AccountHolderName = request.AccountHolderName,
            IBANNumber = request.IBANNumber,
            BICCode = request.BICCode
        };

        _context.BankDetails.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
