using Escrow.Api.Application.Common.Models.Dto;
using Escrow.Api.Infrastructure.Authentication.Services;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using System.Security.Claims;
using Escrow.Api.Domain.Entities.Authentication;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Escrow.Application.Users.EventHandlers;
using Escrow.Api.Application.Authentication.Interfaces;

namespace Escrow.Api.Web.Endpoints.Authentication
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IOtpManagerService _otpManagerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOpenIddictTokenManager _tokenManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(OtpManagerService otpManagerService, 
            UserManager<ApplicationUser> userManager, 
            IOpenIddictTokenManager tokenManager,
            ILogger<AuthController> logger)
        {
            _otpManagerService = otpManagerService;
            _userManager = userManager;
            _tokenManager = tokenManager;
            _logger = logger;
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
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { Error = "Invalid OTP or user ID could not be retrieved." });

                // Find the user using the retrieved user ID
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return BadRequest(new { Error = "User not found." });

                // Ensure user data is populated
                if (string.IsNullOrEmpty(user.Id))
                    return BadRequest(new { Error = "User data is incomplete." });

                // Create claims and identity for the token
                var identity = new ClaimsIdentity("otp");
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id));
                //identity.AddClaim(new Claim(OpenIddictConstants.Claims.PhoneNumber, user.PhoneNumber));
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, user.UserName ?? string.Empty));
                
                //CUSTOM property in OpenIdDict
                /*identity.AddClaim(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty));
                // Add any custom claims
                identity.AddClaim(new Claim("user_id", user.Id));*/

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
                _logger.LogError(ex, "An error occurred while verifying OTP.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "An unexpected error occurred.", Details = ex.Message });
            }
        }

    }
}
