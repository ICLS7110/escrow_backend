
using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Authentication.Interfaces;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Escrow.Api.Application.UserPanel.Commands
{
    public class UpdateMobileNumberCommand : IRequest<Result<object>>
    {
        public string? SocialId { get; set; }
        public string? CountryCode { get; set; }
        public string? MobileNumber { get; set; }
    }

    public class UpdateMobileNumberCommandHandler : IRequestHandler<UpdateMobileNumberCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IOtpValidationService _validationService;
        private readonly IOtpService _otpService;
        private readonly IMemoryCache _cache;

        public UpdateMobileNumberCommandHandler(
            IApplicationDbContext context,
            IOtpValidationService validationService,
            IOtpService otpService,
            IMemoryCache cache)
        {
            _context = context;
            _validationService = validationService;
            _otpService = otpService;
            _cache = cache;
        }

        public async Task<Result<object>> Handle(UpdateMobileNumberCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.SocialId) ||
                string.IsNullOrWhiteSpace(request.CountryCode) ||
                string.IsNullOrWhiteSpace(request.MobileNumber))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid request data.");
            }

            var user = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.SocialId == request.SocialId, cancellationToken);

            if (user == null)
                return Result<object>.Failure(StatusCodes.Status404NotFound, "User not found.");

            var phoneNumber = $"{request.CountryCode}{request.MobileNumber}";
            var isPhoneNumberValid = await _validationService.ValidatePhoneNumberAsync(phoneNumber);
            if (!isPhoneNumberValid)
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid phone number format.");

            var otp = await _otpService.GenerateOtpAsync();
            _cache.Set(phoneNumber, otp, TimeSpan.FromMinutes(5));
            await _otpService.SendOtpAsync(phoneNumber, otp);

            user.PhoneNumber = (request.CountryCode.StartsWith("+") ? request.CountryCode : $"+{request.CountryCode}")
                               + request.MobileNumber.Replace(" ", "");

            user.IsProfileCompleted = true;
            _context.UserDetails.Update(user);
            await _context.SaveChangesAsync(cancellationToken);


            return Result<object>.Success(StatusCodes.Status200OK, "OTP has been sent to the mobile number.", new { MobileNumber = phoneNumber, UserId = user.Id });
        }
    }
}
