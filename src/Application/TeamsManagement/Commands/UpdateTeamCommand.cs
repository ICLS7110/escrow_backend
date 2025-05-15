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
        public List<string>? ContractId { get; set; }
    }

    //public class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
    //{
    //    public UpdateTeamCommandValidator()
    //    {
    //        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
    //        RuleFor(x => x.TeamId).NotEmpty().WithMessage("Team ID is required.");
    //        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    //        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email format.");
    //        RuleFor(x => x.PhoneNumber).Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.");
    //    }
    //}

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
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.TeamId))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "User ID and Team ID are required.");
            }

            var updatedBy = _jwtService.GetUserId().ToInt();
            if (updatedBy == 0)
            {
                return Result<object>.Failure(StatusCodes.Status401Unauthorized, "Unauthorized request.");
            }

            var user = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(request.UserId), cancellationToken);

            if (user == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "User not found.");
            }

            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(t => t.UserId == request.UserId && t.Id == Convert.ToInt32(request.TeamId), cancellationToken);

            if (teamMember == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "Team member not found for the given Team ID.");
            }

            // 🔹 Update user fields
            user.FullName = request.Name ?? user.FullName;
            user.EmailAddress = request.Email ?? user.EmailAddress;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.LastModified = DateTime.UtcNow;
            user.LastModifiedBy = updatedBy.ToString();

            // 🔹 Handle ContractId List<string> -> string (comma-separated)
            string? contractIds = request.ContractId != null
                ? string.Join(",", request.ContractId)
                : teamMember.ContractId;

            teamMember.RoleType = request.RoleType ?? teamMember.RoleType;
            teamMember.ContractId = contractIds;
            teamMember.LastModified = DateTime.UtcNow;
            teamMember.LastModifiedBy = updatedBy.ToString();

            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, "Team updated successfully.", new
            {
                request.UserId,
                request.TeamId,
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.RoleType,
                ContractId = request.ContractId,
                UpdatedBy = updatedBy
            });
        }

       
    }
}
