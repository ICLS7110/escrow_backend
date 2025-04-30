using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.Authentication;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.UserPanel.Commands.CreateUser;

public class SocialLoginCommand : IRequest<Result<UserLoginDto>>
{
    public string Provider { get; set; } = string.Empty; // Google or Apple
    //public string Token { get; set; } = string.Empty; // ID Token from Google/Apple
    public string? SId { get; set; }
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
            // Validate token manually or via external service (Assuming validation is already done)
            if (string.IsNullOrEmpty(request.SId))
            {
                return Result<UserLoginDto>.Failure(StatusCodes.Status400BadRequest, "Invalid social login token.");
            }

            // Check if user already exists
            var user = await _context.UserDetails.FirstOrDefaultAsync(u => u.SocialId == request.SId, cancellationToken);

            var now = AsUnspecified(DateTime.UtcNow);

            if (user == null)
            {
                user = new UserDetail
                {
                    UserId = Guid.NewGuid().ToString(),
                    FullName = string.Empty,
                    EmailAddress = string.Empty,
                    LoginMethod = request.Provider,
                    IsActive = true,
                    IsProfileCompleted = false,
                    Created = now,
                    Role = nameof(Roles.User),
                    SocialId = request.SId,
                };

                _context.UserDetails.Add(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                user.LoginMethod = request.Provider ?? user.LoginMethod;
                user.SocialId = request.SId ?? user.SocialId;
                user.Created = DateTimeOffset.UtcNow;
                user.LastModified = DateTimeOffset.UtcNow; // or DateTimeOffset.Now depending on your need
                user.LastModifiedBy = user.Id.ToString();

                //_context.UserDetails.Update(user);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Generate JWT Token
            var token = _jwtService.GetJWT(user.Id.ToString());

            var userDto = new UserLoginDto
            {
                UserId = user.Id.ToString(),
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

