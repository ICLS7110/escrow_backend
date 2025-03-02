using System;
using System.Threading.Tasks;
using Escrow.Api.Application;
using Escrow.Api.Application.Authentication.Interfaces;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.UserPanel;
using Microsoft.AspNetCore.Http;
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
        _cache = cache ?? throw new ArgumentNullException(nameof(cache)); ;
    }

    // Implementing RequestOtpAsync from IOtpManagerService
    public async Task<bool> RequestOtpAsync(string countryCode, string mobileNumber)
    {        
        var phoneNumber = $"{countryCode}{mobileNumber}";
        var isPhoneNumberValid = await _validationService.ValidatePhoneNumberAsync(phoneNumber);
        if (!isPhoneNumberValid)
            return false;   
        
       
        var otp = await _otpService.GenerateOtpAsync();

        // Store OTP in cache with a 5-minute expiration
        _cache.Set(phoneNumber, otp, TimeSpan.FromMinutes(5));

        // Send the OTP
        await _otpService.SendOtpAsync(phoneNumber, otp);
        return true ;

    }

    // Implementing VerifyOtpAsync from IOtpManagerService
    //public async Task<UserDetail> VerifyOtpAsync(string countryCode, string mobileNumber, string otp)
    //{
    //    var phoneNumber = $"{countryCode}{mobileNumber}";
    //    if (!_cache.TryGetValue(phoneNumber, out object? cachedOtp) || cachedOtp is not string storedOtp)
    //        throw new ArgumentException("OTP expired or invalid.");

    //    if (storedOtp != otp)
    //        throw new ArgumentException("Invalid OTP.");

    //    // Remove OTP after successful validation
    //    _cache.Remove(phoneNumber);

    //    var user = await _userService.FindUserAsync(phoneNumber);
    //    if (user == null)
    //        return new UserDetail();

    //    if (string.IsNullOrEmpty(user.UserId))
    //        return new UserDetail();

    //    return user;
    //}
    public async Task<Result<UserDetail>> VerifyOtpAsync(string countryCode, string mobileNumber, string otp)
    {
       
        var phoneNumber = $"{countryCode}{mobileNumber}";

        if (!_cache.TryGetValue(phoneNumber, out object? cachedOtp) || cachedOtp is not string storedOtp)
            return Result<UserDetail>.Failure(StatusCodes.Status400BadRequest, "OTP expired or invalid.");

        if (storedOtp != otp)
            return Result<UserDetail>.Failure(StatusCodes.Status400BadRequest, "Invalid OTP.");

        // Remove OTP after successful validation
        _cache.Remove(phoneNumber);

        // Try to find the user by phone number
        var userRes = await _userService.FindUserAsync(phoneNumber);

        // If the user doesn't exist, create a new one
        if (userRes.Data == null)
        {

            // Save the new user to the database (assuming _userService has a CreateUserAsync method)
            userRes = await _userService.CreateUserAsync(phoneNumber);
        }

        if (userRes.Data is null)
              return Result<UserDetail>.Failure(StatusCodes.Status404NotFound, $"Not Found");

        return Result<UserDetail>.Success(StatusCodes.Status200OK, $"User creation Success", userRes.Data); 
    }


}
