using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Infrastructure.Security;
using Escrow.Api.Application.Exceptions;

namespace Escrow.Api.Application.BankDetails.Commands;
public record UpdateBankDetailCommand: IRequest<int>
{
    public int Id { get; set; }    
    public string AccountHolderName { get; set; } = string.Empty;
    public string IBANNumber { get; set; } = string.Empty;
    public string BICCode { get; set; } = string.Empty;
    public string BankName { get; set; } = String.Empty;
}

public class UpdateBankDetailCommandHandler : IRequestHandler<UpdateBankDetailCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IAESService _aESService;
    private readonly IJwtService _jwtService;

    public UpdateBankDetailCommandHandler(IApplicationDbContext context, IAESService aESService,IJwtService jwtService)
    {
        _context = context;
       _aESService = aESService;
        _jwtService = jwtService;
    }

    public async Task<int> Handle(UpdateBankDetailCommand request, CancellationToken cancellationToken)
    {
        var userid = _jwtService.GetUserId().ToInt();
        var entity = await _context.BankDetails.FirstOrDefaultAsync(x => x.Id==request.Id && x.UserDetailId== userid);

        if (entity == null)
        {
            throw new EscrowDataNotFoundException("Bank Details Not Found.");
        }        
        entity.AccountHolderName = request.AccountHolderName;
        entity.IBANNumber =_aESService.Encrypt( request.IBANNumber);
        entity.BICCode = request.BICCode;
        entity.BankName = _aESService.Encrypt(request.BankName);
        
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
// Compare this snippet from src/Application/BankDetails/Commands/DeleteBankDetail.cs:
