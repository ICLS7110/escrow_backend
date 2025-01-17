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
                // Verify the OTP and retrieve the user ID
                var userId = await _otpManagerService.VerifyOtpAsync(request.MobileNumber, request.Otp);

                // Find the user using the retrieved user ID
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return BadRequest(new { Error = "User not found." });

                // Ensure user data is populated
                if (string.IsNullOrEmpty(user.UserName))
                    return BadRequest(new { Error = "User data is incomplete." });

                // Create claims and identity for the token
                var identity = new ClaimsIdentity("otp");
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id));
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Username, user.UserName));

                var principal = new ClaimsPrincipal(identity);

                // Generate the access token
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "An unexpected error occurred.", Details = ex.Message });
            }
        }

    }
}
