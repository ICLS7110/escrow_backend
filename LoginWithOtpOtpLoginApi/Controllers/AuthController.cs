using OpenIddict.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OtpLoginApi.Models;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Cryptography;
using OtpLoginApi.Services;
using Twilio.Jwt.AccessToken;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Npgsql.Replication.PgOutput.Messages;

namespace OtpLoginApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOpenIddictTokenManager _tokenManager;
        private readonly TwilioSettings _twilioSettings;
        private readonly PhoneNumberValidationService _phoneNumberValidationService;

        private static readonly ConcurrentDictionary<string, (string otp, DateTime expiry)> _otpStore = new();

        public AuthController(UserManager<ApplicationUser> userManager, IOpenIddictTokenManager tokenManager, IOptions<TwilioSettings> twilioSettings, PhoneNumberValidationService phoneNumberValidationService)
        {
            _userManager = userManager;
            _tokenManager = tokenManager;
            _twilioSettings = twilioSettings.Value;
            _phoneNumberValidationService = phoneNumberValidationService;
        }

        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto request)
        {
            
            if (!_phoneNumberValidationService.ValidatePhoneNumber(request.MobileNumber))
            {
                return BadRequest(new { Error = "Invalid phone number." });
            }

            var user = await _userManager.FindByNameAsync(request.MobileNumber);
            if (user == null)
            {
                user = new ApplicationUser { UserName = request.MobileNumber, PhoneNumber = request.MobileNumber };
                await _userManager.CreateAsync(user);
            }

            var otp = GenerateOtp();

            _otpStore[request.MobileNumber] = (otp, DateTime.UtcNow.AddMinutes(5));

            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);

            try
            {
                var message = MessageResource.Create(
                    body: $"Your OTP is {otp}",
                    from: new PhoneNumber(_twilioSettings.PhoneNumber),
                    to: new PhoneNumber(request.MobileNumber)
                );

                Console.WriteLine($"OTP sent to {request.MobileNumber}: {otp}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to send OTP", Details = ex.Message });
            }

            return Ok(new { Success = true, Message = "OTP sent successfully." });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
        {
        
            if (!_otpStore.ContainsKey(request.MobileNumber) || _otpStore[request.MobileNumber].expiry < DateTime.UtcNow)
            {
                return BadRequest(new { Error = "OTP has expired or does not exist." });
            }

            var storedOtp = _otpStore[request.MobileNumber].otp;

            // Validate OTP
            if (request.Otp != storedOtp)
            {
                return BadRequest(new { Error = "Invalid OTP." });
            }

            // Remove OTP after successful verification (for security)
            _otpStore.TryRemove(request.MobileNumber, out _);

            // Find the user
            var user = await _userManager.FindByNameAsync(request.MobileNumber);
            if (user == null)
                return BadRequest(new { Error = "User not found." });

            var identity = new ClaimsIdentity("otp");
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id));
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Username, user.UserName));

            var principal = new ClaimsPrincipal(identity);

            // Issue an access token
            var token = await _tokenManager.CreateAsync(new OpenIddictTokenDescriptor
            {
                Principal = principal,
                Subject = user.Id,
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                Type = OpenIddictConstants.TokenTypes.Bearer
            });

            return Ok(new { Success = true, Message = "Verification Successfull", AccessToken = token });
        }

        private string GenerateOtp()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] byteArray = new byte[6];
                rng.GetBytes(byteArray);
                var otp = BitConverter.ToUInt32(byteArray, 0) % 1000000; // Ensure it is 6 digits
                var expiryTime = DateTime.UtcNow.AddMinutes(1);
                return otp.ToString("D6");
            }
        }

        [HttpPost("resend-otp")]
        public IActionResult ResendOtp([FromBody] RequestOtpDto request)
        {
            // Validate the mobile number format
            if (!_phoneNumberValidationService.ValidatePhoneNumber(request.MobileNumber))
            {
                return BadRequest(new { Error = "Invalid phone number." });
            }

            // Check if OTP exists and is valid
            if (_otpStore.TryGetValue(request.MobileNumber, out var existingOtp))
            {
                // Check if OTP has already been verified (i.e., already used or expired)
                if (existingOtp.expiry > DateTime.UtcNow)
                {
                    // Resend the existing OTP only if it's not verified or expired
                    try
                    {
                        SendOtp(request.MobileNumber, existingOtp.otp);
                        return Ok(new { Message = "OTP resent successfully." });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { Error = "Failed to resend OTP", Details = ex.Message });
                    }
                }
                else
                {
                    _otpStore.TryRemove(request.MobileNumber, out _);
                }
            }

            var newOtp = GenerateOtp();
            _otpStore[request.MobileNumber] = (newOtp, DateTime.UtcNow.AddMinutes(5));

            try
            {
                SendOtp(request.MobileNumber, newOtp);
                return Ok(new { Success = true, Message = "New OTP sent successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Failed to send OTP", Details = ex.Message });
            }
        }


        private void SendOtp(string mobileNumber, string otp)
        {
            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);

            var message = MessageResource.Create(
                body: $"Your OTP is {otp}",
                from: new PhoneNumber(_twilioSettings.PhoneNumber),
                to: new PhoneNumber(mobileNumber)
            );

            Console.WriteLine($"OTP sent to {mobileNumber}: {otp}");
        }

    }
}
