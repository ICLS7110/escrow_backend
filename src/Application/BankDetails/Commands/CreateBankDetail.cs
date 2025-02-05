using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Infrastructure.Security;


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
    private readonly IRsaHelper _rsaHelper;

    public CreateBankDetailCommandHandler(IApplicationDbContext context, IRsaHelper rsaHelper)
    {
        _context = context;
        _rsaHelper = rsaHelper;
    }

    public async Task<int> Handle(CreateBankDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = new BankDetail
        {
            UserDetailId = request.UserDetailId,
            AccountHolderName = request.AccountHolderName,
            IBANNumber =_rsaHelper.EncryptWithPrivateKey( request.IBANNumber),
            BICCode = _rsaHelper.EncryptWithPrivateKey(request.BICCode)
        };

        _context.BankDetails.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
