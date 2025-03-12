using Escrow.Api.Application.Common.Models.Dto;
using Escrow.Api.Infrastructure.Authentication.Services;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using System.Security.Claims;

using Escrow.Api.Application.Authentication.Interfaces;
using Microsoft.IdentityModel.Tokens;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Text;
using Escrow.Api.Web.Helpers;
using System.IdentityModel.Tokens.Jwt;

using Escrow.Api.Domain.Entities.UserPanel;

using Escrow.Api.Application.DTOs;

namespace Escrow.Api.Web.Endpoints.Authentication
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IOtpManagerService _otpManagerService;
        private readonly UserManager<ApplicationUser1> _userManager;
        //private readonly IOpenIddictTokenManager _tokenManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;

        public AuthController(OtpManagerService otpManagerService, 
            UserManager<ApplicationUser1> userManager,
            
            IUserService userService,
            ILogger<AuthController> logger)
        {
            _otpManagerService = otpManagerService;
            _userManager = userManager;
            
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("request-otp")]
        public async Task<IResult> RequestOtp([FromBody] RequestOtpDto request)
        {
            var isValid = await _otpManagerService.RequestOtpAsync(request.CountryCode,request.MobileNumber);
            if (!isValid) 
                return TypedResults.Ok(Result<object>.Failure(StatusCodes.Status400BadRequest, "Mobile Number Not Valid."));
            
            return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "OTP sent successfully.", new()));          
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
        {
            
                // Verify the OTP and retrieve the user ID
            var res = await _otpManagerService.VerifyOtpAsync(request.countryCode,request.MobileNumber, request.Otp);
            if (res.Data is null)
                return Ok(res);

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
            if(res.Data.Id == 0 && !String.IsNullOrEmpty(res.Data.PhoneNumber))
            {
                var userDetail = await _userService.FindOrCreateUserAsync(res.Data.PhoneNumber);
                newUser = userDetail == null ? new UserDetail() : res.Data;
            }              

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, res.Data.Id.ToString()),
                new Claim(ClaimTypes.Role,"User")
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

            object result = new
            {               
                AccessToken = token,
                UserId = String.IsNullOrEmpty(newUser.PhoneNumber) ? res.Data.Id : newUser.Id,
                //IsProfileCompleted = user?.IsProfileCompleted
            };
            return Ok(Result<object>.Success(StatusCodes.Status200OK, "OTP verified successfully.", result));               
            
        }

    }
}
