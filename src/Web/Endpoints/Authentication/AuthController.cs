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
using Microsoft.IdentityModel.Tokens;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Text;
using Escrow.Api.Web.Helpers;
using System.IdentityModel.Tokens.Jwt;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Web.Endpoints.Authentication
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IOtpManagerService _otpManagerService;
        private readonly UserManager<ApplicationUser> _userManager;
        //private readonly IOpenIddictTokenManager _tokenManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;

        public AuthController(OtpManagerService otpManagerService, 
            UserManager<ApplicationUser> userManager,
            //IOpenIddictTokenManager tokenManager,
            IUserService userService,
            ILogger<AuthController> logger)
        {
            _otpManagerService = otpManagerService;
            _userManager = userManager;
            //_tokenManager = tokenManager;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("request-otp")]
        public async Task<IResult> RequestOtp([FromBody] RequestOtpDto request)
        {
            try
            {
                await _otpManagerService.RequestOtpAsync(request.CountryCode,request.MobileNumber);
                return TypedResults.Ok(new { Message = "OTP sent successfully.", status = 200 });
                //return Ok(new { status=200,Message = "OTP sent successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying OTP.===>" + ex.Message);
                return TypedResults.Problem($"An error occurred: {ex.Message}");
                //return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
        {
            try
            {
                // Verify the OTP and retrieve the user ID
                var user = await _otpManagerService.VerifyOtpAsync(request.countryCode,request.MobileNumber, request.Otp);
                if (string.IsNullOrEmpty(user.UserId))
                    return BadRequest(new { Error = "Invalid OTP or user ID could not be retrieved." });

                #region Commented Old Code
                //// Create claims and identity for the token
                //var identity = new ClaimsIdentity("otp");
                //identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.UserId));
                ////identity.AddClaim(new Claim(OpenIddictConstants.Claims.PhoneNumber, user.PhoneNumber));
                //identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, user.FullName ?? string.Empty));

                ////CUSTOM property in OpenIdDict
                ///*identity.AddClaim(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty));
                //// Add any custom claims
                //identity.AddClaim(new Claim("user_id", user.Id));*/

                //var principal = new ClaimsPrincipal(identity);

                //// Generate the access token
                //var token = await _tokenManager.CreateAsync(new OpenIddictTokenDescriptor
                //{
                //    Principal = principal,
                //    Subject = user.UserId,
                //    ExpirationDate = DateTime.UtcNow.AddDays(1),
                //    Type = OpenIddictConstants.TokenTypes.Bearer
                //});
                #endregion

                #region OpenIdDict New R&D
                //var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

                //// Use the client_id as the subject identifier.
                //identity.SetClaim(Claims.Subject, user.Id);
                //identity.SetClaim(Claims.Name, user.FullName);

                ///*identity.SetDestinations(static claim => claim.Type switch
                //{
                //    // Allow the "name" claim to be stored in both the access and identity tokens
                //    // when the "profile" scope was granted (by calling principal.SetScopes(...)).
                //    Claims.Name when claim.Subject.HasScope(Scopes.Profile)
                //        => [Destinations.AccessToken, Destinations.IdentityToken],

                //    // Otherwise, only store the claim in the access tokens.
                //    _ => [Destinations.AccessToken]
                //});*/

                //var tokenOptions = new OpenIddictTokenDescriptor
                //{
                //    Principal = new ClaimsPrincipal(identity),
                //    Subject = user.Id.ToString(),
                //    ExpirationDate = DateTime.UtcNow.AddDays(1),
                //    Type = OpenIddictConstants.TokenTypes.Bearer
                //};

                //var token = await _tokenManager.CreateAsync(tokenOptions);
                #endregion

                //Fetching user again because at the time of creation Primary key is not available due to commit transaction
                UserDetail newUser = new UserDetail();
                if(user.Id == 0 && !String.IsNullOrEmpty(user.PhoneNumber))
                {
                    var userDetail = await _userService.FindUserAsync(user.PhoneNumber);
                    newUser = userDetail == null ? new UserDetail() : userDetail;
                }
                

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

                var secrectKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationHelper.JwtIssuerSigningKey));
                var siginginCredentials = new SigningCredentials(secrectKey, SecurityAlgorithms.HmacSha256);
                var tokenOptions = new JwtSecurityToken(
                    issuer: ConfigurationHelper.JwtValidIssuer,
                    audience: ConfigurationHelper.JwtValidAudience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(ConfigurationHelper.AuthTokenExpiry),
                    signingCredentials: siginginCredentials
                );
                var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                return Ok(new
                {
                    status = 200,
                    Message = "OTP verified successfully.",
                    AccessToken = token,
                    UserId = String.IsNullOrEmpty(newUser.PhoneNumber) ? user.Id : newUser.Id,
                    IsProfileCompleted = false//user.AccountHolderName != null
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
