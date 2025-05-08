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
}






