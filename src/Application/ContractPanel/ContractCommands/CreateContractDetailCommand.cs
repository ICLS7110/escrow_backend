using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Domain.Entities.ContractPanel;

namespace Escrow.Api.Application.ContractPanel.ContractCommands;
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

    public List<MileStoneDTO>? MileStones { get; set; }
}

public class CreateContractDetailsHandler : IRequestHandler<CreateContractDetailCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    public CreateContractDetailsHandler(IApplicationDbContext applicationDbContext, IJwtService jwtService, IMapper mapper)
    {
        _context = applicationDbContext;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateContractDetailCommand request, CancellationToken cancellationToken)
    {
        int ContractId;
        int userid = _jwtService.GetUserId().ToInt();
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
            BuyerDetailsId = request.Role == EscrowApIConstant.ContratConstant.ContractRoleBuyer ? userid : null,
            SellerDetailsId = request.Role == EscrowApIConstant.ContratConstant.ContractRoleSeller ? userid : null,
            UserDetailId = userid           
        };
        await _context.ContractDetails.AddAsync(entity);
        await _context.SaveChangesAsync(cancellationToken);
        ContractId = entity.Id;
        if (request.MileStones != null && request.MileStones.Any())
        {
            foreach (var milestone in request.MileStones)
            {
                var mappedentity = _mapper.Map<MileStone>(milestone);
                mappedentity.CreatedBy = userid.ToString();
                mappedentity.ContractId = ContractId;
                await _context.MileStones.AddAsync(mappedentity);
            }
        }
        await _context.SaveChangesAsync(cancellationToken);
        return ContractId;
    }
}
