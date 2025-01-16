using Escrow.Api.Application.Common.Models.Dto;
using Escrow.Api.Application.Common.Services;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace Escrow.Api.Web.Endpoints.Authentication
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly OtpManagerService _otpManagerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOpenIddictTokenManager _tokenManager;

        public AuthController(OtpManagerService otpManagerService, UserManager<ApplicationUser> userManager, IOpenIddictTokenManager tokenManager)
        {
            _otpManagerService = otpManagerService;
            _userManager = userManager;
            _tokenManager = tokenManager;
        }

        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto request)
        {
            try
            {
                await _otpManagerService.RequestOtpAsync(request.MobileNumber);
                return Ok(new { Message = "OTP sent successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.MobileNumber);
                if (user == null)
                    return BadRequest(new { Error = "User not found." });

                // Verify OTP
                var isOtpValid = await _otpManagerService.VerifyOtpAsync(request.MobileNumber, request.Otp);
                if (isOtpValid != "Valid")
                {
                    return BadRequest(new { Error = "Invalid OTP." });
                }

                if (string.IsNullOrEmpty(user.Id) || string.IsNullOrEmpty(user.UserName))
                {
                    return BadRequest(new { Error = "User ID or Username is missing." });
                }

                var identity = new ClaimsIdentity("otp");

                // Add claims safely
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id));
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Username, user.UserName));

                var principal = new ClaimsPrincipal(identity);

                var token = await _tokenManager.CreateAsync(new OpenIddictTokenDescriptor
                {
                    Principal = principal,
                    Subject = user.Id,
                    ExpirationDate = DateTime.UtcNow.AddDays(1),
                    Type = OpenIddictConstants.TokenTypes.Bearer
                });

                return Ok(new
                {
                    Message = "OTP verified successfully.",
                    AccessToken = token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

    }
}
