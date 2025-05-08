using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Helpers;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.Authentication;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Escrow.Api.Application.UserPanel.Commands.CreateUser;

public class SocialLoginCommand : IRequest<Result<UserLoginDto>>
{
    public string Provider { get; set; } = string.Empty; // Google or Apple
    public string Token { get; set; } = string.Empty; // ID Token from Google/Apple
    //public string? SId { get; set; }
}

public class SocialLoginCommandHandler : IRequestHandler<SocialLoginCommand, Result<UserLoginDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public SocialLoginCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
    }

    //public static DateTime AsUnspecified(DateTime dateTime) =>
    //DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);


    public static DateTime AsUnspecified(DateTime dateTime) =>
    DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);


    public async Task<Result<UserLoginDto>> Handle(SocialLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 🔐 Step 1: Validate Firebase Token and Extract Claims
            // ✅ Step 1: Validate and extract claims from Firebase token
            var userClaims = await FirebaseTokenHelper.ExtractClaimsAsync(request.Token);
            if (userClaims == null)
            {
                return Result<UserLoginDto>.Failure(StatusCodes.Status400BadRequest, "Invalid or expired Firebase token.");
            }

            string email = userClaims["email"];
            string name = userClaims["name"];
            string? picture = userClaims.GetValueOrDefault("picture");
            string socialId = userClaims["user_id"];

            // 🔎 Step 2: Check if user exists by email
            var user = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == email && !u.IsDeleted, cancellationToken);

            var now = DateTime.UtcNow;

            if (user == null)
            {
                // 🆕 Step 3: Create new user
                user = new UserDetail
                {
                    UserId = Guid.NewGuid().ToString(),
                    FullName = name,
                    EmailAddress = email,
                    ProfilePicture = picture,
                    LoginMethod = request.Provider,
                    IsActive = true,
                    IsProfileCompleted = false,
                    Created = now,
                    Role = nameof(Roles.User),
                    SocialId = socialId,
                };

                _context.UserDetails.Add(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // ✏️ Step 4: Update existing user
                user.FullName ??= name;
                user.ProfilePicture ??= picture;
                user.LoginMethod = request.Provider ?? user.LoginMethod;
                user.SocialId = socialId;
                user.LastModified = now;
                user.LastModifiedBy = user.Id.ToString();
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 🎟 Step 5: Issue JWT token
            var token = _jwtService.GetJWT(user.Id.ToString());

            var userDto = new UserLoginDto
            {
                UserId = user.Id.ToString(),
                EmailAddress = user.EmailAddress,
                FullName = user.FullName,
                Token = token,
                IsProfileCompleted = user.IsProfileCompleted,
            };

            return Result<UserLoginDto>.Success(StatusCodes.Status200OK, "Login successful.", userDto);
        }
        catch (Exception ex)
        {
            return Result<UserLoginDto>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
        }
    }


    //public async Task<Result<UserLoginDto>> Handle(SocialLoginCommand request, CancellationToken cancellationToken)
    //{
    //    await ValidateAndReadClaims("eyJhbGciOiJSUzI1NiIsImtpZCI6IjNmOWEwNTBkYzRhZTgyOGMyODcxYzMyNTYzYzk5ZDUwMjc3ODRiZTUiLCJ0eXAiOiJKV1QifQ.eyJuYW1lIjoiWW9nZXNoIFNoYXJtYSIsInBpY3R1cmUiOiJodHRwczovL2xoMy5nb29nbGV1c2VyY29udGVudC5jb20vYS9BQ2c4b2NKMmFDZ1lCd25lVEJkS0U5alZTUDFLNjExZE5iRGl0YnhQbGc4SnhjRGdYSGZMRVE9czk2LWMiLCJpc3MiOiJodHRwczovL3NlY3VyZXRva2VuLmdvb2dsZS5jb20vd2VsaW5rLXNhLXRlc3QiLCJhdWQiOiJ3ZWxpbmstc2EtdGVzdCIsImF1dGhfdGltZSI6MTc0NjY4MTUyOSwidXNlcl9pZCI6Ill0V3UxSFN3amNZTTZtbjdpWmpRV1V5U09WazIiLCJzdWIiOiJZdFd1MUhTd2pjWU02bW43aVpqUVdVeVNPVmsyIiwiaWF0IjoxNzQ2NjgxNTI5LCJleHAiOjE3NDY2ODUxMjksImVtYWlsIjoieW9nZXNoNzA3MzA2MjYwMEBnbWFpbC5jb20iLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwiZmlyZWJhc2UiOnsiaWRlbnRpdGllcyI6eyJnb29nbGUuY29tIjpbIjExMjY2ODI0MjA4OTE0MTY0MTQzNCJdLCJlbWFpbCI6WyJ5b2dlc2g3MDczMDYyNjAwQGdtYWlsLmNvbSJdfSwic2lnbl9pbl9wcm92aWRlciI6Imdvb2dsZS5jb20ifX0.gnsIy346ha55ITdXuGeUFSWzwKqqMnqlC70YJiV3TLQ5Jng_gMauhfpC6KnxIx-nrTNuhao3e3rtaRWGh4FNgRf_DQt1SahF8bzIWbhGWUmF9tqKWTmzZE6Df38vPPiMYdWzXCGnJ7ok7XwdYzTOIke5J9yFN2VD8HFauW9uqW_oNVZQJSPiIvN023ZFwDkjFKLKuRqAv1ABAKU2WDYWnxJN3fUp_ZJsSejES6YGuG_aEB06IpaNYktPFf8wBNE0R5KZZwdoUUI6QV3jpOxaBePxX2AuPwbpOXGoMH_ysjf8o7iwU-S_AyDoi1yPT9eCV-nFmozwPVErexA9IXo9hg");

    //    try
    //    {
    //        // Validate token manually or via external service (Assuming validation is already done)
    //        if (string.IsNullOrEmpty(request.SId))
    //        {
    //            return Result<UserLoginDto>.Failure(StatusCodes.Status400BadRequest, "Invalid social login token.");
    //        }

    //        // Check if user already exists
    //        var user = await _context.UserDetails.FirstOrDefaultAsync(u => u.SocialId == request.SId, cancellationToken);

    //        var now = AsUnspecified(DateTime.UtcNow);

    //        if (user == null)
    //        {
    //            user = new UserDetail
    //            {
    //                UserId = Guid.NewGuid().ToString(),
    //                FullName = string.Empty,
    //                EmailAddress = string.Empty,
    //                LoginMethod = request.Provider,
    //                IsActive = true,
    //                IsProfileCompleted = false,
    //                Created = now,
    //                Role = nameof(Roles.User),
    //                SocialId = request.SId,
    //            };

    //            _context.UserDetails.Add(user);
    //            await _context.SaveChangesAsync(cancellationToken);
    //        }
    //        else
    //        {
    //            user.LoginMethod = request.Provider ?? user.LoginMethod;
    //            user.SocialId = request.SId ?? user.SocialId;
    //            user.Created = DateTimeOffset.UtcNow;
    //            user.LastModified = DateTimeOffset.UtcNow; // or DateTimeOffset.Now depending on your need
    //            user.LastModifiedBy = user.Id.ToString();

    //            //_context.UserDetails.Update(user);
    //            await _context.SaveChangesAsync(cancellationToken);
    //        }

    //        // Generate JWT Token
    //        var token = _jwtService.GetJWT(user.Id.ToString());

    //        var userDto = new UserLoginDto
    //        {
    //            UserId = user.Id.ToString(),
    //            Token = token,
    //            IsProfileCompleted = user.IsProfileCompleted,
    //        };

    //        return Result<UserLoginDto>.Success(StatusCodes.Status200OK, "Login successful.", userDto);
    //    }
    //    catch (Exception ex)
    //    {
    //        return Result<UserLoginDto>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
    //    }
    //}

    //public static async Task ValidateAndReadClaims(string token)
    //{
    //    var issuer = "https://securetoken.google.com/welink-sa-test"; // 🔁 Replace with your Firebase Project ID
    //    var audience = "welink-sa-test"; // 🔁 Same as Firebase Project ID
    //    var httpClient = new HttpClient();

    //    // ✅ Step 1: Quick format check
    //    if (string.IsNullOrWhiteSpace(token) || token.Split('.').Length != 3)
    //    {
    //        Console.WriteLine("❌ Invalid JWT format.");
    //        return;
    //    }

    //    try
    //    {
    //        // ✅ Step 2: Fetch OpenID configuration
    //        string configUrl = "https://www.googleapis.com/service_accounts/v1/jwk/securetoken@system.gserviceaccount.com";
    //        string keysJson = await httpClient.GetStringAsync(configUrl);
    //        var keys = new JsonWebKeySet(keysJson);

    //        // ✅ Step 3: Setup validation parameters
    //        var validationParameters = new TokenValidationParameters
    //        {
    //            ValidateIssuer = true,
    //            ValidIssuer = issuer,

    //            ValidateAudience = true,
    //            ValidAudience = audience,

    //            ValidateIssuerSigningKey = true,
    //            IssuerSigningKeys = keys.Keys,

    //            ValidateLifetime = true,
    //            ClockSkew = TimeSpan.FromMinutes(5)
    //        };

    //        var handler = new JwtSecurityTokenHandler();

    //        // Optional but helpful: trim and sanity check
    //        token = token.Trim();

    //        if (string.IsNullOrWhiteSpace(token) || token.Split('.').Length != 3)
    //        {
    //            Console.WriteLine("❌ Token is malformed or missing parts.");
    //            return;
    //        }

    //        if (!handler.CanReadToken(token))
    //        {
    //            Console.WriteLine("❌ Token failed CanReadToken() — may be corrupt.");
    //            return;
    //        }


    //        ClaimsPrincipal principal = handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

    //        // ✅ Step 4: Extract required claims
    //        string? email = principal.FindFirst(ClaimTypes.Email)?.Value
    //                     ?? principal.FindFirst("email")?.Value;

    //        string? name = principal.FindFirst(ClaimTypes.Name)?.Value
    //                    ?? principal.FindFirst("name")?.Value;

    //        Console.WriteLine("✅ Token is valid.");
    //        Console.WriteLine($"📧 Email: {email}");
    //        Console.WriteLine($"👤 Name: {name}");

    //        // Optional: Print all claims
    //        Console.WriteLine("\nAll Claims:");
    //        foreach (var claim in principal.Claims)
    //        {
    //            Console.WriteLine($"{claim.Type}: {claim.Value}");
    //        }
    //    }
    //    catch (SecurityTokenException ex)
    //    {
    //        Console.WriteLine($"❌ Token validation failed: {ex.Message}");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"❌ Unexpected error: {ex.Message}");
    //    }
    //}
}

//public static async Task ValidateAndReadClaims(string token)
//{
//    var issuer = "https://accounts.google.com";
//    var audience = "44473283156-7op213480o9p3jagklfremnp8qunugm3.apps.googleusercontent.com"; // your Google Client ID


//    var httpClient = new HttpClient();

//    try
//    {
//        // 1. Fetch OpenID config
//        string configJson;
//        try
//        {
//            configJson = await httpClient.GetStringAsync($"{issuer}/.well-known/openid-configuration");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"❌ Failed to fetch OpenID configuration: {ex.Message}");
//            return;
//        }

//        JObject config;
//        try
//        {
//            config = JObject.Parse(configJson);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"❌ Failed to parse OpenID configuration: {ex.Message}");
//            return;
//        }

//        var jwksUriToken = config["jwks_uri"];
//        if (jwksUriToken == null)
//        {
//            Console.WriteLine("❌ 'jwks_uri' not found in OpenID configuration.");
//            return;
//        }

//        var jwksUri = jwksUriToken.ToString();

//        // 2. Fetch signing keys
//        string keysJson;
//        try
//        {
//            keysJson = await httpClient.GetStringAsync(jwksUri);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"❌ Failed to fetch JWKS from URI: {ex.Message}");
//            return;
//        }

//        JsonWebKeySet keys;
//        try
//        {
//            keys = new JsonWebKeySet(keysJson);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"❌ Failed to parse JWKS: {ex.Message}");
//            return;
//        }

//        // 3. Validate JWT
//        var tokenHandler = new JwtSecurityTokenHandler();
//        var validationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidIssuer = issuer,

//            ValidateAudience = true,
//            ValidAudience = audience,

//            ValidateIssuerSigningKey = true,
//            IssuerSigningKeys = keys.Keys,

//            ValidateLifetime = true
//        };

//        try
//        {
//            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
//            Console.WriteLine("✅ Token is valid. Claims:");
//            foreach (var claim in principal.Claims)
//            {
//                Console.WriteLine($"{claim.Type}: {claim.Value}");
//            }
//        }
//        catch (SecurityTokenException ex)
//        {
//            Console.WriteLine($"❌ Token validation failed: {ex.Message}");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"❌ Unexpected error during token validation: {ex.Message}");
//        }
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"❌ General error: {ex.Message}");
//    }
//}




