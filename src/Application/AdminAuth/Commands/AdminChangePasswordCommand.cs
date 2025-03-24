using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.AdminPanel;

namespace Escrow.Api.Application.AdminAuth.Commands
{
    public record AdminChangePasswordCommand : IRequest<Result<object>>
    {
        public string Email { get; init; } = string.Empty;
        public string OldPassword { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
        public string ConfirmNewPassword { get; init; } = string.Empty;
    }

    public class AdminChangePasswordCommandHandler : IRequestHandler<AdminChangePasswordCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;

        public AdminChangePasswordCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<object>> Handle(AdminChangePasswordCommand request, CancellationToken cancellationToken)
        {
            // Validate input fields
            if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "Old password and new password are required.");
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "New password and confirm password do not match.");
            }

            // Fetch admin user
            var adminUser = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (adminUser == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "Admin user not found.");
            }

            // Ensure new password is not the same as old password
            if (adminUser.PasswordHash == request.NewPassword)
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "New password cannot be the same as the old password.");
            }

            // Update password
            adminUser.PasswordHash = request.NewPassword;
            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, "Password changed successfully.");
        }

    }
}
