using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Infrastructure.Security;

namespace Escrow.Api.Application.BankDetails.Commands;
public record UpdateBankDetailCommand: IRequest<int>
{
    public int Id { get; set; }
    public int UserDetailId { get; set; }
    public string AccountHolderName { get; set; } = string.Empty;
    public string IBANNumber { get; set; } = string.Empty;
    public string BICCode { get; set; } = string.Empty;
}

public class UpdateBankDetailCommandHandler : IRequestHandler<UpdateBankDetailCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IAESService _aESService;

    public UpdateBankDetailCommandHandler(IApplicationDbContext context, IAESService aESService)
    {
        _context = context;
       _aESService = aESService;
    }

    public async Task<int> Handle(UpdateBankDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.BankDetails.FindAsync(request.Id);

        if (entity == null)
        {
            throw new NotFoundException(nameof(BankDetail), request.Id.ToString());
        }

        entity.UserDetailId = request.UserDetailId;
        entity.AccountHolderName = request.AccountHolderName;
        entity.IBANNumber =_aESService.Encrypt( request.IBANNumber);
        entity.BICCode = _aESService.Encrypt(request.BICCode);
        
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
// Compare this snippet from src/Application/BankDetails/Commands/DeleteBankDetail.cs:
