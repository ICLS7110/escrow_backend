using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.Exceptions;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.ContractPanel.ContractCommands;

public record EditContractDetailCommand : IRequest<int>
{
    public int Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string ContractTitle { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;
    public string? AdditionalNote { get; set; }
    public string FeesPaidBy { get; set; } = string.Empty;
    public decimal? FeeAmount { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerMobile { get; set; }
    public string? SellerName { get; set; }
    public string? SellerMobile { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class EditContractDetailCommandHandler : IRequestHandler<EditContractDetailCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public EditContractDetailCommandHandler(IApplicationDbContext applicationDbContext, IJwtService jwtService)
    {
        _context = applicationDbContext;
        _jwtService = jwtService;
    }

    public async Task<int> Handle(EditContractDetailCommand request, CancellationToken cancellationToken)
    {
        int userid = _jwtService.GetUserId().ToInt();
        var entity = await _context.ContractDetails.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserDetailId == userid);
        if (entity == null)
        {
            throw new EscrowDataNotFoundException("Contract Not Found.");
        }

        entity.Role = request.Role;
        entity.ContractTitle = request.ContractTitle;
        entity.ServiceType = request.ServiceType;
        entity.ServiceDescription = request.ServiceDescription;
        entity.AdditionalNote = request.AdditionalNote;
        entity.FeesPaidBy = request.FeesPaidBy;
        entity.FeeAmount = request.FeeAmount;
        entity.BuyerName = request.BuyerName;
        entity.BuyerMobile = request.BuyerMobile;
        entity.SellerMobile = request.SellerMobile;
        entity.SellerName = request.SellerName;
        entity.Status = request.Status;
        entity.BuyerDetailsId = request.Role == EscrowApIConstant.ContratConstant.ContractRoleBuyer ? userid : null;
        entity.SellerDetailsId = request.Role == EscrowApIConstant.ContratConstant.ContractRoleSeller ? userid : null;

        _context.ContractDetails.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
