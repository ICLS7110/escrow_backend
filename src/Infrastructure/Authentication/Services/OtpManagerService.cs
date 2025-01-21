using System;
using System.Threading.Tasks;
using Escrow.Api.Application.Authentication.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Escrow.Api.Infrastructure.Authentication.Services;

public class OtpManagerService : IOtpManagerService
{
    private readonly IMemoryCache _cache;
    private readonly IOtpService _otpService;
    private readonly IOtpValidationService _validationService;
    private readonly IUserService _userService;

    public OtpManagerService(
        IOtpService otpService,
        IOtpValidationService validationService,
        IUserService userService,
        IMemoryCache cache)
    {
        _otpService = otpService;
        _validationService = validationService;
        _userService = userService;
        _cache = cache;
    }

    // Implementing RequestOtpAsync from IOtpManagerService
    public async Task RequestOtpAsync(string countryCode, string mobileNumber)
    {
        var phoneNumber = $"{countryCode}{mobileNumber}";
        var isPhoneNumberValid = await _validationService.ValidatePhoneNumberAsync(phoneNumber);
        if (!isPhoneNumberValid)
            throw new ArgumentException("Invalid phone number.");

        var otp = await _otpService.GenerateOtpAsync();

        // Store OTP in cache with a 5-minute expiration
        _cache.Set(phoneNumber, otp, TimeSpan.FromMinutes(5));

        // Send the OTP
        await _otpService.SendOtpAsync(phoneNumber, otp);
    }

    // Implementing VerifyOtpAsync from IOtpManagerService
    public async Task<string> VerifyOtpAsync(string countryCode, string mobileNumber, string otp)
    {
        var phoneNumber = $"{countryCode}{mobileNumber}";
        if (!_cache.TryGetValue(phoneNumber, out object? cachedOtp) || cachedOtp is not string storedOtp)
            throw new ArgumentException("OTP expired or invalid.");

        if (storedOtp != otp)
            throw new ArgumentException("Invalid OTP.");

        // Remove OTP after successful validation
        _cache.Remove(phoneNumber);

        var user = await _userService.FindUserAsync(phoneNumber);
        if (user == null)
            throw new ArgumentException("User not found.");

        if (string.IsNullOrEmpty(user.UserId))
            throw new ArgumentException("User Id is not populated.");

        return user.UserId;
    }
}
