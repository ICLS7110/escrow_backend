
using Escrow.Api.Application.Authentication.Interfaces;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Events.UserPanel;
using System.Threading;
using Microsoft.EntityFrameworkCore;

using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Domain.Enums;


namespace Escrow.Api.Infrastructure.Authentication.Services
{
    public class UserService : IUserService
    {
        private readonly IApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserService(IApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Result<UserDetail>> FindOrCreateUserAsync(string phoneNumber)
        {
            var user = await _context.UserDetails.FindAsync(phoneNumber);
            if (user == null)
            {
                var entity = new UserDetail
                {
                    PhoneNumber = phoneNumber,
                    Role=nameof(Roles.User)
                };
                
                //_context.UserDetails.Add(entity);
                user = await _context.SaveChangesAndReturnAsync(entity);

                // Check if creation was successful
                if (user == null)
                    return Result<UserDetail>.Failure(StatusCodes.Status500InternalServerError, $"User creation failed.");
            }

            return Result<UserDetail>.Success(StatusCodes.Status200OK, $"User creation Success", user);
        }

        public async Task<Result<UserDetail>> FindUserAsync(string phoneNumber)
        {
            var user = await _context.UserDetails.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
            if (user == null)
                return Result<UserDetail>.Failure(StatusCodes.Status404NotFound, $"Not Found");
            return Result<UserDetail>.Success(StatusCodes.Status200OK, $"User creation Success", user); 
        }

        public async Task<Result<UserDetail>> CreateUserAsync(string phoneNumber)
        {
            var existingUser = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (existingUser != null)
            {
                return Result<UserDetail>.Success(StatusCodes.Status200OK, "User already exists", existingUser);
            }

            var newUser = new UserDetail
            {
                UserId = Guid.NewGuid().ToString(),
                FullName = "",
                PhoneNumber = phoneNumber,
                Role = nameof(Roles.User),
                IsActive = true,
                IsProfileCompleted = false,
                IsDeleted = false,
                Created = DateTime.UtcNow,
            };

            await _context.UserDetails.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return Result<UserDetail>.Success(StatusCodes.Status200OK, "User created successfully", newUser);
        }

        //public async Task<Result<UserDetail>> CreateUserAsync(string phoneNumber)
        //{
        //    var existingApplicationUser = await _userManager.Users.Where(u => u.PhoneNumber == phoneNumber).FirstOrDefaultAsync();   
        //    ApplicationUser? newApplicationUser;
        //    if (existingApplicationUser == null)
        //    {
        //        // Create a new user
        //        newApplicationUser = new ApplicationUser
        //        {
        //            UserName = phoneNumber,
        //            PhoneNumber = phoneNumber,
        //            PhoneNumberConfirmed = true
        //        };

        //        var createUserResult = await _userManager.CreateAsync(newApplicationUser);
        //        if (!createUserResult.Succeeded)
        //        {
        //            var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
        //            return Result<UserDetail>.Failure(StatusCodes.Status500InternalServerError, $"User creation failed: {errors}");
        //        }

        //        // Assign "User" role
        //        var roleResult = await _userManager.AddToRoleAsync(newApplicationUser, "User");
        //        if (!roleResult.Succeeded)
        //        {
        //            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
        //            return Result<UserDetail>.Failure(StatusCodes.Status500InternalServerError, $"Role assignment failed: {roleErrors}");

        //        }
        //        newApplicationUser = await _userManager.FindByNameAsync(phoneNumber);
        //    }
        //    else
        //    {
        //        newApplicationUser = existingApplicationUser;
        //    }
        //    var newUser = new UserDetail
        //    {
        //        PhoneNumber = phoneNumber,
        //        UserId = newApplicationUser?.Id ?? Guid.Empty.ToString(),
        //    };

        //    _context.UserDetails.Add(newUser);
        //    await _context.SaveChangesAsync();

        //    if (newApplicationUser == null)
        //    {
        //        return Result<UserDetail>.Failure(StatusCodes.Status500InternalServerError, $"User creation failed");

        //    }
        //    return Result<UserDetail>.Success(StatusCodes.Status200OK, $"User creation Success", newUser);
        //}
    }
}
