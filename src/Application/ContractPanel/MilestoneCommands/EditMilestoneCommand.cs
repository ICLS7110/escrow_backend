using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.ContractPanel.MilestoneCommands;
public record EditMilestoneCommand : IRequest<Result<int>>
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public string MilestoneTitle { get; set; } = string.Empty;
    public string MilestoneDescription { get; set; } = string.Empty;
    public DateTimeOffset DueDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class EditMilestoneCommandHandler : IRequestHandler<EditMilestoneCommand, Result<int>>
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

    public async Task<Result<int>> Handle(EditMilestoneCommand request, CancellationToken cancellationToken)
    {
        int userid = _jwtService.GetUserId().ToInt();
        var entity = await _context.MileStones.FirstOrDefaultAsync(x => x.Id == request.Id && x.CreatedBy == userid.ToString() && x.ContractId == request.ContractId);
        if (entity == null)
        {
            return Result<int>.Failure(StatusCodes.Status404NotFound, "Milestone Not Found.");
        }

        entity.Name = request.MilestoneTitle;
        entity.Description = request.MilestoneDescription;
        entity.DueDate = request.DueDate;
        entity.Amount = request.Amount;

        _context.MileStones.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(StatusCodes.Status200OK, "Success", entity.Id);
    }
}
