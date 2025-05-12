using System;
using System.Linq;
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
    public record AdminForgotPasswordCommand : IRequest<Result<object>> // Changed Result<string> to Result<object>
    {
        public string Email { get; init; } = string.Empty;
    }

    public class AdminForgotPasswordCommandHandler : IRequestHandler<AdminForgotPasswordCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;

        public AdminForgotPasswordCommandHandler(
            IApplicationDbContext context,
            IJwtService jwtService, IEmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }


        public async Task<Result<object>> Handle(AdminForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // Check if the email exists
            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

            if (adminUser == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "Email not found.");
            }

            // Generate and save OTP
            string otp = new Random().Next(100000, 999999).ToString();
            adminUser.OTP = otp;
            await _context.SaveChangesAsync(cancellationToken);

            string userName = adminUser.FullName ?? "Admin";

            // Send email with OTP
            string subject = "Your OTP for Password Reset";
            string body = $@"
    <p>Dear {userName},</p>
    <p>You requested to reset your password. Use the following OTP to proceed:</p>
    <h2>{otp}</h2>
    <p>This OTP will expire shortly. Do not share it with anyone.</p>";


            // Check if the email address is valid
            if (string.IsNullOrWhiteSpace(adminUser.EmailAddress))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "User does not have a valid email address.");
            }

            await _emailService.SendEmailAsync(adminUser.EmailAddress, subject, userName, body);


            return Result<object>.Success(StatusCodes.Status200OK, "OTP sent successfully.", new { });
        }

        //public async Task<Result<object>> Handle(AdminForgotPasswordCommand request, CancellationToken cancellationToken)
        //{
        //    // Check if the email exists
        //    var adminUser = await _context.UserDetails
        //        .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

        //    if (adminUser == null)
        //    {
        //        return Result<object>.Failure(StatusCodes.Status404NotFound, "Email not found.");
        //    }

        //    string otp = "1234";//new Random().Next(100000, 999999).ToString();

        //    adminUser.OTP = otp;
        //    await _context.SaveChangesAsync(cancellationToken);

        //    return Result<object>.Success(StatusCodes.Status200OK, "OTP sent successfully.", new { });
        //}
    }
}

