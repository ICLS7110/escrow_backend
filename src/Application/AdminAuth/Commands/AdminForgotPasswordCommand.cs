
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
using Escrow.Api.Application.Common.Constants;
using Microsoft.Extensions.Localization;
using Escrow.Api.Application.Common.Helpers;
using Amazon.Runtime.Internal.Util;
using PhoneNumbers;
using Microsoft.Extensions.Caching.Memory;

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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;


        public AdminForgotPasswordCommandHandler(
            IApplicationDbContext context,
            IJwtService jwtService,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache cache)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        public async Task<Result<object>> Handle(AdminForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // Check if HttpContext is available and then get the language
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;  // Use null-coalescing operator to fall back to English

            // Check if the email exists
            var adminUser = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

            if (adminUser == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("EmailNotFound", language));
            }

            // Generate and save OTP
            string otp = new Random().Next(100000, 999999).ToString();
            adminUser.OTP = otp;


            _cache.Set(request.Email, otp, TimeSpan.FromMinutes(5));
            await _context.SaveChangesAsync(cancellationToken);

            string userName = adminUser.FullName ?? "Admin";

            // Send email with OTP
            string subject = AppMessages.Get("YourOtpForPasswordReset", language);
            string body = $@"
    <p>{AppMessages.Get("Dear", language)} {userName},</p>
    <p>{AppMessages.Get("YouRequestedToResetYourPassword", language)} {AppMessages.Get("UseTheFollowingOtpToProceed", language)}:</p>
    <h2>{otp}</h2>
    <p>{AppMessages.Get("ThisOtpWillExpireShortly", language)}. {AppMessages.Get("DoNotShareItWithAnyone", language)}.</p>";

            // Check if the email address is valid
            if (string.IsNullOrWhiteSpace(adminUser.EmailAddress))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("UserDoesNotHaveValidEmailaddress", language));
            }

            await _emailService.SendEmailAsync(adminUser.EmailAddress, subject, userName, body);

            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("OtpSentSuccessfully", language), new { });
        }
    }
}








































//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using MediatR;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Domain.Entities.AdminPanel;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Http;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Application.Common.Constants;

//namespace Escrow.Api.Application.AdminAuth.Commands
//{
//    public record AdminForgotPasswordCommand : IRequest<Result<object>> // Changed Result<string> to Result<object>
//    {
//        public string Email { get; init; } = string.Empty;
//    }

//    public class AdminForgotPasswordCommandHandler : IRequestHandler<AdminForgotPasswordCommand, Result<object>>
//    {
//        private readonly IApplicationDbContext _context;
//        private readonly IJwtService _jwtService;
//        private readonly IEmailService _emailService;

//        public AdminForgotPasswordCommandHandler(
//            IApplicationDbContext context,
//            IJwtService jwtService, IEmailService emailService)
//        {
//            _context = context;
//            _jwtService = jwtService;
//            _emailService = emailService;
//        }


//        public async Task<Result<object>> Handle(AdminForgotPasswordCommand request, CancellationToken cancellationToken)
//        {
//            // Check if the email exists
//            var adminUser = await _context.UserDetails
//                .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

//            if (adminUser == null)
//            {
//                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.EmailNotFound);
//            }

//            // Generate and save OTP
//            string otp = new Random().Next(100000, 999999).ToString();
//            adminUser.OTP = otp;
//            await _context.SaveChangesAsync(cancellationToken);

//            string userName = adminUser.FullName ?? "Admin";

//            // Send email with OTP
//            string subject = "Your OTP for Password Reset";
//            string body = $@"
//    <p>Dear {userName},</p>
//    <p>You requested to reset your password. Use the following OTP to proceed:</p>
//    <h2>{otp}</h2>
//    <p>This OTP will expire shortly. Do not share it with anyone.</p>";


//            // Check if the email address is valid
//            if (string.IsNullOrWhiteSpace(adminUser.EmailAddress))
//            {
//                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.UserDoesNotHaveValidEmailaddress);
//            }

//            await _emailService.SendEmailAsync(adminUser.EmailAddress, subject, userName, body);


//            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.OtpSentSuccessfully, new { });
//        }

//        //public async Task<Result<object>> Handle(AdminForgotPasswordCommand request, CancellationToken cancellationToken)
//        //{
//        //    // Check if the email exists
//        //    var adminUser = await _context.UserDetails
//        //        .FirstOrDefaultAsync(u => u.EmailAddress == request.Email, cancellationToken);

//        //    if (adminUser == null)
//        //    {
//        //        return Result<object>.Failure(StatusCodes.Status404NotFound, "Email not found.");
//        //    }

//        //    string otp = "1234";//new Random().Next(100000, 999999).ToString();

//        //    adminUser.OTP = otp;
//        //    await _context.SaveChangesAsync(cancellationToken);

//        //    return Result<object>.Success(StatusCodes.Status200OK, "OTP sent successfully.", new { });
//        //}
//    }
//}

