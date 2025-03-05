using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Domain.Entities.ContractPanel;

namespace Escrow.Api.Application.ContractPanel.MilestoneCommands;

public record CreateMilestoneCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public string? Documents { get; set; }

    public int? ContractId { get; set; }

    //public ContractDetailsDTO? ContractDetails { get; set; }
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
        int MilestoneId;
        int userid = _jwtService.GetUserId().ToInt();
        var entity = new MileStone
        {
            Name = request.Name,
            Amount = request.Amount,
            Description = request.Description,
            DueDate = request.DueDate,
            Documents = request.Documents,
            ContractId = request.ContractId           
        };
        await _context.MileStones.AddAsync(entity);
        await _context.SaveChangesAsync(cancellationToken);
        MilestoneId = entity.Id;
        return MilestoneId;
    }
}
