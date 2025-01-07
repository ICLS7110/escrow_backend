using Escrow.Application.Dto;
using Escrow.Application.Services;
using Escrow.Domain.UserPanel;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace Escrow.Api.Controllers
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
                return BadRequest(new
                {
                    status = 400,
                    statusText = "BAD_REQUEST",
                    message = "Invalid phone number.",
                    data = new
                    {
                        error = "Invalid phone number."
                    }
                });

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
                return StatusCode(500, new
                {
                    status = 500,
                    statusText = "INTERNAL_SERVER_ERROR",
                    message = "Failed to send OTP",
                    data = new
                    {
                        error = "Failed to send OTP",
                        details = ex.Message
                    }
                });

            }

            return Ok(new
            {
                status = 200,
                statusText = "SUCCESS",
                message = "OTP sent successfully.",
                data = new
                {
                    success = true
                }
            });

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
                return BadRequest(new
                {
                    status = 400,
                    statusText = "BAD_REQUEST",
                    message = "Invalid OTP.",
                    data = new
                    {
                        error = "Invalid OTP."
                    }
                });

            }

            // Remove OTP after successful verification (for security)
            _otpStore.TryRemove(request.MobileNumber, out _);

            // Find the user
            var user = await _userManager.FindByNameAsync(request.MobileNumber);
            if (user == null)
                return BadRequest(new
                {
                    status = 400,
                    statusText = "BAD_REQUEST",
                    message = "User not found.",
                    data = new
                    {
                        error = "User not found."
                    }
                });


            var identity = new ClaimsIdentity("otp");
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id));
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Username, user.UserName));

            var principal = new ClaimsPrincipal(identity);

            // Issue an access token
            var token = await _tokenManager.CreateAsync(new OpenIddictTokenDescriptor
            {
                Principal = principal,
                Subject = user.Id,
                CreationDate= DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddDays(1),
                Type = OpenIddictConstants.TokenTypes.Bearer
            });

            return Ok(new
            {
                status = 200,
                statusText = "SUCCESS",
                message = "Verification Successful",
                data = new
                {
                    success = true,
                    accessToken = token
                }
            });

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
                        return Ok(new
                        {
                            status = 200,
                            statusText = "SUCCESS",
                            message = "OTP resent successfully.",
                            data = new
                            {
                                success = true
                            }
                        });

                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new
                        {
                            status = 500,
                            statusText = "INTERNAL_SERVER_ERROR",
                            message = "Failed to resend OTP",
                            data = new
                            {
                                error = "Failed to resend OTP",
                                details = ex.Message
                            }
                        });

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
                return Ok(new
                {
                    status = 200,
                    statusText = "SUCCESS",
                    message = "New OTP sent successfully.",
                    data = new
                    {
                        success = true
                    }
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = 500,
                    statusText = "INTERNAL_SERVER_ERROR",
                    message = "Failed to send OTP",
                    data = new
                    {
                        error = "Failed to send OTP",
                        details = ex.Message
                    }
                });

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
