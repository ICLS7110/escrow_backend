using Escrow.Api.Domain.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Services
{
    public class OtpManagerService : IOtpManagerService
    {
        private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expiry)> _otpStore = new();

        private readonly IOtpService _otpService;
        private readonly IOtpValidationService _validationService;
        private readonly IUserService _userService;

        public OtpManagerService(IOtpService otpService, IOtpValidationService validationService, IUserService userService)
        {
            _otpService = otpService;
            _validationService = validationService;
            _userService = userService;
        }

        // Implementing RequestOtpAsync from IOtpManagerService
        public async Task RequestOtpAsync(string phoneNumber)
        {
            if (!_validationService.ValidatePhoneNumber(phoneNumber))
                throw new ArgumentException("Invalid phone number.");

            await _userService.FindOrCreateUserAsync(phoneNumber);

            var otp = _otpService.GenerateOtp();
            _otpStore[phoneNumber] = (otp, DateTime.UtcNow.AddMinutes(5));

            _otpService.SendOtp(phoneNumber, otp);
        }

        // Implementing VerifyOtpAsync from IOtpManagerService
        public async Task<string> VerifyOtpAsync(string phoneNumber, string otp)
        {
            if (!_otpStore.TryGetValue(phoneNumber, out var storedOtp) || storedOtp.Expiry < DateTime.UtcNow)
                throw new ArgumentException("OTP expired or invalid.");

            if (storedOtp.Otp != otp)
                throw new ArgumentException("Invalid OTP.");

            _otpStore.TryRemove(phoneNumber, out _);

            var user = await _userService.FindUserAsync(phoneNumber);
            if (user == null)
                throw new ArgumentException("User not found.");

            if (string.IsNullOrEmpty(user.Id))
                throw new ArgumentException("User Id is not populated.");

            return user.Id;
        }
    }
}
