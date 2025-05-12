using System;
using System.Threading.Tasks;
using Escrow.Api.Application;
using Escrow.Api.Application.Authentication.Interfaces;
using Escrow.Api.Application.Common.Helpers;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.UserPanel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Escrow.Api.Infrastructure.Authentication.Services;

public class OtpManagerService : IOtpManagerService
{
    private readonly IMemoryCache _cache;
    private readonly IOtpService _otpService;
    private readonly IOtpValidationService _validationService;
    private readonly IUserService _userService;
    private readonly UnifonicSmsService _smsService;

    public OtpManagerService(
        IOtpService otpService,
        IOtpValidationService validationService,
        IUserService userService,
        IMemoryCache cache,
        UnifonicSmsService smsService)
    {
        _otpService = otpService;
        _validationService = validationService;
        _userService = userService;
        _cache = cache ?? throw new ArgumentNullException(nameof(cache)); 
        _smsService = smsService;
    }

    // Implementing RequestOtpAsync from IOtpManagerService
    public async Task<bool> RequestOtpAsync(string countryCode, string mobileNumber)
    {        
        var phoneNumber = $"{countryCode}{mobileNumber}";
        var isPhoneNumberValid = await _validationService.ValidatePhoneNumberAsync(phoneNumber);
        if (!isPhoneNumberValid)
            return false;
       
            

            //var success = await _smsService.SendSmsAsync(request.To, request.Message);
      

        var message = "Hello from Unifonic sandbox!";
        var response = _smsService.SendSmsAsync(phoneNumber, message,"");
        Console.WriteLine($"SMS sent: {response}");


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
    public bool VerifyOtpAsync(string phoneNumber, string otp)
    {

        if (!_cache.TryGetValue(phoneNumber, out object? cachedOtp) || cachedOtp is not string storedOtp)
            return false;

        if (storedOtp != otp)
            return false;

    
        _cache.Remove(phoneNumber);

        return true;
    }


}
