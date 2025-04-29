using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.TeamsManagement.Commands;

public class UpdateTeamStatusCommand : IRequest<Result<bool>>
{
    public int TeamId { get; set; }
}

public class UpdateTeamStatusHandler : IRequestHandler<UpdateTeamStatusCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public UpdateTeamStatusHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<bool>> Handle(UpdateTeamStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.GetUserId();
        if (userId == null)
        {
            return Result<bool>.Failure(StatusCodes.Status401Unauthorized, "Unauthorized user.");
        }

        try
        {
            var team = await _context.TeamMembers // <-- Ensure you're querying the correct table
                .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

            if (team == null)
            {
                return Result<bool>.Failure(StatusCodes.Status404NotFound, "Team not found.");
            }

            // Toggle IsActive status safely
            team.IsActive = !team.IsActive;

            var changes = await _context.SaveChangesAsync(cancellationToken);
            if (changes == 0)
            {
                return Result<bool>.Failure(StatusCodes.Status500InternalServerError, "Failed to update team status.");
            }

            return Result<bool>.Success(StatusCodes.Status200OK, "Team status updated successfully.", true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating team status: {ex.Message}");
            return Result<bool>.Failure(StatusCodes.Status500InternalServerError, "An unexpected error occurred while updating the team status.");
        }
    }
}
