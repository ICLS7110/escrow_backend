using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.AdminPanel;
using System.Numerics;

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
        private readonly IPasswordHasher _passwordHasher;

        public AdminChangePasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<object>> Handle(AdminChangePasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "Old password and new password are required.");
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "New password and confirm password do not match.");
            }

            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

            if (adminUser == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "Admin user not found.");
            }

            if (string.IsNullOrEmpty(adminUser.Password) ||
                !_passwordHasher.VerifyPassword(request.OldPassword, adminUser.Password))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "Old password is incorrect.");
            }

            if (_passwordHasher.VerifyPassword(request.NewPassword, adminUser.Password))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "New password cannot be the same as the old password.");
            }

            adminUser.Password = _passwordHasher.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, "Password changed successfully.");
        }
    }
}
