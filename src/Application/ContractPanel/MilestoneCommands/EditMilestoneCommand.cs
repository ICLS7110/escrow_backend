using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.ContractPanel.MilestoneCommands;
public record EditMilestoneCommand : IRequest<Result<List<int>>>
{
    public List<MilestoneUpdateDTO> Milestones { get; set; } = new();
}


public class EditMilestoneCommandHandler : IRequestHandler<EditMilestoneCommand, Result<List<int>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;

    public EditMilestoneCommandHandler(IApplicationDbContext context, IMapper mapper, IJwtService jwtService)
    {
        _context = context;
        _mapper = mapper;
        _jwtService = jwtService;
    }

    public async Task<Result<List<int>>> Handle(EditMilestoneCommand request, CancellationToken cancellationToken)
    {
        int userId = _jwtService.GetUserId().ToInt();
        var updatedMilestoneIds = new List<int>();

        foreach (var milestone in request.Milestones)
        {
            var entity = await _context.MileStones
                .FirstOrDefaultAsync(x => x.Id == milestone.Id && x.CreatedBy == userId.ToString() && x.ContractId == milestone.ContractId, cancellationToken);

            if (entity == null)
            {
                continue; // Skip if not found
            }

            entity.Name = milestone.MilestoneTitle;
            entity.Description = milestone.MilestoneDescription;
            entity.DueDate = milestone.DueDate;
            entity.Amount = milestone.Amount;

            _context.MileStones.Update(entity);
            updatedMilestoneIds.Add(entity.Id);
        }

        if (!updatedMilestoneIds.Any())
        {
            return Result<List<int>>.Failure(StatusCodes.Status404NotFound, "No milestones were updated.");
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<List<int>>.Success(StatusCodes.Status200OK, "Milestones updated successfully.", updatedMilestoneIds);
    }
}
