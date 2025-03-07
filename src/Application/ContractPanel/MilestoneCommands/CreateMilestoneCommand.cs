using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.ContractPanel;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.ContractPanel.MilestoneCommands;

public record CreateMilestoneCommand : IRequest<int>
{
    public List<MileStoneDTO>? MileStoneDetails { get; set; }
    public int? ContractId { get; set; }
    public decimal? TaxAmount { get; set; }
    public int? EscrowTax { get; set; }
    public int? EscrowFeeAmount { get; set; }
}

public class CreateMilestoneCommandHandler : IRequestHandler<CreateMilestoneCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    public CreateMilestoneCommandHandler(IApplicationDbContext applicationDbContext, IJwtService jwtService, IMapper mapper)
    {
        _context = applicationDbContext;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateMilestoneCommand request, CancellationToken cancellationToken)
    {
        int userid = _jwtService.GetUserId().ToInt();
        List<MileStoneDTO>? list = request?.MileStoneDetails;
        for (int i = 0; i < list?.Count; i++)
        {
            MileStoneDTO? item = list[i];
            var entity = new MileStone
            {
                Name = item.Name,
                Amount = item.Amount,
                Description = item.Description,
                DueDate = item.DueDate,
                Documents = item.Documents,
                ContractId = item.ContractId
            };
            await _context.MileStones.AddAsync(entity);
        }

        
        await _context.SaveChangesAsync(cancellationToken);
        //MilestoneId = entity.Id;


        if (request?.ContractId != null)
        {
            var Contract = await _context.ContractDetails.FirstOrDefaultAsync(x => x.CreatedBy == userid.ToString() && x.Id == request.ContractId);
            if (Contract == null)
            {
                return StatusCodes.Status404NotFound;
            }

            Contract.EscrowTax = request.EscrowTax;
            Contract.FeeAmount = request.EscrowFeeAmount;
            //Contract.EscrowTax = request.EscrowTax;

            _context.ContractDetails.Update(Contract);
            await _context.SaveChangesAsync(cancellationToken);
        }
        await _context.SaveChangesAsync(cancellationToken);
        return StatusCodes.Status201Created;
    }
}
