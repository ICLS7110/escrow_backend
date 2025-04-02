using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.TeamsManagement.Commands
{
    public class UpdateTeamCommand : IRequest<Result<object>>
    {
        public string UserId { get; set; } = string.Empty;
        public string TeamId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RoleType { get; set; }
        public string? ContractId { get; set; }
    }

    public class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;  // ✅ Injected JWT Service

        public UpdateTeamCommandHandler(IApplicationDbContext context, IJwtService jwtService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public async Task<Result<object>> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
        {
            // 🔹 Validate input
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.TeamId))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "User ID and Team ID are required.");
            }

            // ✅ Fetch the current authenticated user
            var updatedBy = _jwtService.GetUserId().ToInt();
            if (updatedBy == 0)
            {
                return Result<object>.Failure(StatusCodes.Status401Unauthorized, "Unauthorized request.");
            }

            // 🔹 Find the user detail
            var user = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(request.UserId), cancellationToken);

            if (user == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "User not found.");
            }

            // 🔹 Find the team member record by UserId and TeamId
            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(t => t.UserId == request.UserId && t.Id == Convert.ToInt32(request.TeamId), cancellationToken);

            if (teamMember == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "Team member not found for the given Team ID.");
            }

            // 🔹 Update user details
            user.FullName = request.Name ?? user.FullName;
            user.EmailAddress = request.Email ?? user.EmailAddress;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.LastModified = DateTime.UtcNow;
            user.LastModifiedBy = updatedBy.ToString();  // ✅ Now tracking who updated this user

            // 🔹 Update team member details (including RoleType & ContractId)
            teamMember.RoleType = request.RoleType ?? teamMember.RoleType;
            teamMember.ContractId = request.ContractId ?? teamMember.ContractId;
            teamMember.LastModified = DateTime.UtcNow;
            teamMember.LastModifiedBy = updatedBy.ToString();  // ✅ Now tracking who updated this team member

            // 🔹 Save changes
            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, "Team updated successfully.", new
            {
                request.UserId,
                request.TeamId,
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.RoleType,
                request.ContractId,
                UpdatedBy = updatedBy  // ✅ Returning UpdatedBy info
            });
        }
    }
}
