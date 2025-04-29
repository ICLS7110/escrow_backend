using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.AdminAuth.Commands
{
    public class AdminVerifyOTPCommand : IRequest<Result<string>>
    {
        public string Email { get; init; } = string.Empty;
        public string OTP { get; init; } = string.Empty;
    }

    public class AdminVerifyOTPCommandHandler : IRequestHandler<AdminVerifyOTPCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;

        public AdminVerifyOTPCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<string>> Handle(AdminVerifyOTPCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.OTP))
            {
                return Result<string>.Failure(StatusCodes.Status400BadRequest, "Email and OTP are required.");
            }

            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

            if (adminUser == null)
            {
                return Result<string>.Failure(StatusCodes.Status404NotFound, "Admin not found.");
            }

            if (adminUser.OTP != request.OTP)
            {
                return Result<string>.Failure(StatusCodes.Status400BadRequest, "Invalid OTP.");
            }

            return Result<string>.Success(StatusCodes.Status200OK, "OTP verified successfully.");
        }
    }
}
