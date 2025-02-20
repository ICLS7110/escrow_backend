using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using Escrow.Api.Infrastructure.Security;


namespace Escrow.Api.Application.BankDetails.Commands;
public record CreateBankDetailCommand : IRequest<int>
{
    public string AccountHolderName { get; set; } = string.Empty;
    public string IBANNumber { get; set; } = string.Empty;
    public string BICCode { get; set; } = string.Empty;
    public string BankName { get; set; } = String.Empty;
}

public class CreateBankDetailCommandHandler : IRequestHandler<CreateBankDetailCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IAESService _AESService;
    private readonly IJwtService _jwtService;

    public CreateBankDetailCommandHandler(IApplicationDbContext context, IAESService aESService,IJwtService jwtService)
    {
        _context = context;
        _AESService = aESService;
        _jwtService = jwtService;
    }

    public async Task<int> Handle(CreateBankDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = new BankDetail
        {
            UserDetailId = _jwtService.GetUserId().ToInt(),
            AccountHolderName = request.AccountHolderName,
            IBANNumber = _AESService.Encrypt( request.IBANNumber),
            BankName = _AESService.Encrypt(request.BankName),
            BICCode = request.BICCode,            
        };

        _context.BankDetails.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        int userId = _jwtService.GetUserId().ToInt();
        var userentity = await _context.UserDetails.FindAsync(new object[] { userId }, cancellationToken);
        if (userentity != null) 
        { 
            userentity.IsProfileCompleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
        return entity.Id;
    }
}
