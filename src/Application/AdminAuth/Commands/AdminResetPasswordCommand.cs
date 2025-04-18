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

        public AdminResetPasswordCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<object>> Handle(AdminResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

            if (adminUser == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "Admin not found.");
            }

            if (adminUser.OTP != request.OTP)
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid OTP.");
            }

            adminUser.Password = request.NewPassword;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, "Password reset successfully.", new { });
        }
    }
}
