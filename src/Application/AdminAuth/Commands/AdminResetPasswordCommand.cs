using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.AdminPanel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Application.DTOs;

namespace Escrow.Api.Application.AdminAuth.Commands
{
    public class AdminResetPasswordCommand : IRequest<Result<object>>
    {
        public string Email { get; init; } = string.Empty;
        public string OTP { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
    }

    public class AdminResetPasswordCommandHandler : IRequestHandler<AdminResetPasswordCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public AdminResetPasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<object>> Handle(AdminResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

            if (adminUser == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "Admin not found.");
            }

            if (string.IsNullOrEmpty(adminUser.OTP) || adminUser.OTP != request.OTP)
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid OTP.");
            }

            adminUser.Password = _passwordHasher.HashPassword(request.NewPassword);

            // Optional: Invalidate OTP after use
            adminUser.OTP = null;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, "Password reset successfully.", new { });
        }
    }

}
