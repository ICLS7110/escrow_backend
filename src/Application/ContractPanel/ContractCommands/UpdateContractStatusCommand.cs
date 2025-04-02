using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.ContractPanel.ContractCommands;

public class UpdateContractStatusCommand : IRequest<bool>
{
    public int ContractId { get; set; }
    public string BuyerPhoneNumber { get; set; } = string.Empty;
    public string SellerPhoneNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? StatusReason { get; set; }
}

public class UpdateContractStatusCommandHandler : IRequestHandler<UpdateContractStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;

    public UpdateContractStatusCommandHandler(IApplicationDbContext context, IMapper mapper, IJwtService jwtService)
    {
        _context = context;
        _mapper = mapper;
        _jwtService = jwtService;
    }

    public async Task<bool> Handle(UpdateContractStatusCommand request, CancellationToken cancellationToken)
    {
        int userId = _jwtService.GetUserId().ToInt(); // Get the authenticated user's ID

        var buyer = await _context.UserDetails
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.BuyerPhoneNumber, cancellationToken);

        var seller = await _context.UserDetails
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.SellerPhoneNumber, cancellationToken);

        if (buyer == null || seller == null) return false; // Return false if buyer or seller is not found

        var contract = await _context.ContractDetails
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

        if (contract == null) return false; // Return false if the contract is not found

        // Update contract status details
        contract.Status = request.Status;
        contract.StatusReason = request.StatusReason;
        contract.LastModified = DateTime.UtcNow;
        contract.LastModifiedBy = userId.ToString(); // Store the user ID who modified the contract

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
