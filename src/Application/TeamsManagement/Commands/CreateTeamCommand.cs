using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.TeamMembers;
using Escrow.Api.Domain.Entities.UserPanel;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.TeamsManagement.Commands
{
    public class CreateTeamCommand : IRequest<Result<object>>
    {
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RoleType { get; set; }
        public string? ContractId { get; set; }
    }

    public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public CreateTeamCommandHandler(IApplicationDbContext context, IJwtService jwtService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public async Task<Result<object>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
        {
            // ✅ Validate required fields
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "Team name is required.");

            // ✅ Get authenticated user ID from JWT
            var userIdFromJwt = _jwtService.GetUserId();
            if (!int.TryParse(userIdFromJwt, out int createdBy) || createdBy == 0)
                return Result<object>.Failure(StatusCodes.Status401Unauthorized, "Unauthorized request.");

            // ✅ Validate RoleType
            if (string.IsNullOrWhiteSpace(request.RoleType))
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "RoleType is required.");

            // ✅ Validate ContractId (optional but recommended check)
            if (string.IsNullOrWhiteSpace(request.ContractId))
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "ContractId is required.");

            // ✅ Check if a user with the same email already exists
            UserDetail? existingUser = null;
            string userId = string.Empty;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                existingUser = await _context.UserDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);
            }

            if (existingUser == null)
            {
                // ✅ Create a new UserDetail entry
                var newUser = new UserDetail
                {
                    FullName = request.Name,
                    EmailAddress = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    IsActive = true,
                    Created = DateTime.UtcNow,
                    CreatedBy = createdBy.ToString()
                };

                _context.UserDetails.Add(newUser);
                await _context.SaveChangesAsync(cancellationToken);

                userId = newUser.Id.ToString() ?? string.Empty; // Ensure UserId is not null
            }
            else
            {
                userId = existingUser.Id.ToString() ?? string.Empty;
            }

            // ✅ Ensure userId is valid
            if (string.IsNullOrEmpty(userId))
                return Result<object>.Failure(StatusCodes.Status500InternalServerError, "Failed to retrieve UserId.");

            // ✅ Check if the user is already a team member
            bool isAlreadyTeamMember = await _context.TeamMembers
                .AsNoTracking()
                .AnyAsync(tm => tm.UserId == userId, cancellationToken);

            if (isAlreadyTeamMember)
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "This user is already a team member.");

            // ✅ Create a new TeamMember entry
            var teamMember = new TeamMember
            {
                UserId = userId,
                RoleType = request.RoleType,
                ContractId = request.ContractId,
                Created = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = createdBy.ToString()
            };

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, "Team member created successfully.", new
            {
                UserId = userId,
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.RoleType,
                request.ContractId,
                CreatedBy = createdBy
            });
        }
    }
}
