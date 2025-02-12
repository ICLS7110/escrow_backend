using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.ContractPanel;

namespace Escrow.Api.Application.ContractPanel;
public record CreateContractDetailCommand : IRequest<int>
{
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

public class CreateContractDetailsHandler : IRequestHandler<CreateContractDetailCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    public CreateContractDetailsHandler(IApplicationDbContext applicationDbContext, IJwtService jwtService)
    {
        _context = applicationDbContext;
        _jwtService = jwtService;
    }

    public async Task<int> Handle(CreateContractDetailCommand request,CancellationToken cancellationToken)
    {
        var entity = new ContractDetails
        {
            Role = request.Role,
            ContractTitle = request.ContractTitle,
            ServiceType = request.ServiceType,
            ServiceDescription = request.ServiceDescription,
            AdditionalNote = request.AdditionalNote,
            FeesPaidBy = request.FeesPaidBy,
            FeeAmount = request.FeeAmount,
            BuyerName = request.BuyerName,
            BuyerMobile = request.BuyerMobile,
            SellerMobile = request.SellerMobile,
            SellerName = request.SellerName,
            Status = request.Status,
            BuyerDetailsId = request.Role == EscrowApIConstant.ContratConstant.ContractRoleBuyer ?  Convert.ToInt32(_jwtService.GetUserId()) : null,
            SellerDetailsId = request.Role == EscrowApIConstant.ContratConstant.ContractRoleSeller ? Convert.ToInt32(_jwtService.GetUserId()) : null,
            UserDetailId= Convert.ToInt32(_jwtService.GetUserId())
        };
        await _context.ContractDetails.AddAsync(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
