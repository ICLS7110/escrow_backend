using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.ContractPanel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.ContractPanel.MilestoneCommands;

public record CreateMilestoneCommand : IRequest<Result<object>>
{
    public List<MileStoneDTO>? MileStoneDetails { get; set; }
    public int? ContractId { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? EscrowTax { get; set; }
    public int? EscrowFeeAmount { get; set; }
}

public class CreateMilestoneCommandHandler : IRequestHandler<CreateMilestoneCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public CreateMilestoneCommandHandler(IApplicationDbContext applicationDbContext, IJwtService jwtService)
    {
        _context = applicationDbContext;
        _jwtService = jwtService;
    }

    public async Task<Result<object>> Handle(CreateMilestoneCommand request, CancellationToken cancellationToken)
    {
        int userId = _jwtService.GetUserId().ToInt();

        // ✅ Validate input
        if (request?.MileStoneDetails == null || !request.MileStoneDetails.Any())
        {
            return Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid request data. Milestone details are required.");
        }

        foreach (var item in request.MileStoneDetails)
        {
            if (item.Id > 0)
            {
                // ✅ Update existing milestone
                var existingMilestone = await _context.MileStones
                    .FirstOrDefaultAsync(x => x.Id == item.Id && x.ContractId == item.ContractId, cancellationToken);

                if (existingMilestone != null)
                {
                    existingMilestone.Name = item.Name;
                    existingMilestone.Amount = item.Amount;
                    existingMilestone.Description = item.Description;
                    existingMilestone.DueDate = item.DueDate;
                    existingMilestone.Documents = item.Documents;

                    _context.MileStones.Update(existingMilestone);
                }
            }
            else
            {
                // ✅ Add new milestone
                var newMilestone = new MileStone
                {
                    Name = item.Name,
                    Amount = item.Amount,
                    Description = item.Description,
                    DueDate = item.DueDate,
                    Documents = item.Documents,
                    ContractId = item.ContractId,
                    CreatedBy = userId.ToString(),
                    Created = DateTime.UtcNow
                };

                await _context.MileStones.AddAsync(newMilestone, cancellationToken);
            }
        }

        // ✅ Update contract details if necessary
        if (request.ContractId.HasValue)
        {
            var contract = await _context.ContractDetails
                .FirstOrDefaultAsync(x => x.CreatedBy == userId.ToString() && x.Id == request.ContractId.Value, cancellationToken);

            if (contract == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "Contract not found or unauthorized.");
            }

            contract.EscrowTax = request.EscrowTax;
            contract.FeeAmount = request.EscrowFeeAmount;

            _context.ContractDetails.Update(contract);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<object>.Success(StatusCodes.Status200OK, "Milestones created/updated successfully.", new { });
    }
}
