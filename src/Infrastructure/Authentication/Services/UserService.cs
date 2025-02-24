
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.Interfaces;
using Escrow.Api.Domain.Entities;

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

        public async Task<User> FindOrCreateUserAsync(string phoneNumber)
        {
            var user = await _context.UserDetails.FindAsync(phoneNumber);
            if (user == null)
            {
                var entity = new User
                {
                    PhoneNumber = phoneNumber
                };
                
                //_context.UserDetails.Add(entity);
                user = await _context.SaveChangesAndReturnAsync(entity);

                // Check if creation was successful
                if (user == null)
                {
                    throw new InvalidOperationException("User creation failed.");
                }
            }

            return user;
        }

        public async Task<User?> FindUserAsync(string phoneNumber)
        {
            var user = await _context.UserDetails.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);

            // Return null if the user is not found
            return user;
        }


        public async Task<User> CreateUserAsync(string phoneNumber)
        {
            var existingApplicationUser = await _userManager.FindByNameAsync(phoneNumber);
            ApplicationUser? newApplicationUser;
            if (existingApplicationUser == null)
            {
                // Create a new user
                newApplicationUser = new ApplicationUser
                {
                    UserName = phoneNumber,
                    PhoneNumber = phoneNumber,
                    PhoneNumberConfirmed = true
                };

                var createUserResult = await _userManager.CreateAsync(newApplicationUser);
                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    throw new Exception($"User creation failed: {errors}");
                }

                // Assign "User" role
                var roleResult = await _userManager.AddToRoleAsync(newApplicationUser, "User");
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new Exception($"Role assignment failed: {roleErrors}");
                }
                newApplicationUser = await _userManager.FindByNameAsync(phoneNumber);
            }
            else
            {
                newApplicationUser = existingApplicationUser;
            }
            var newUser = new User
            {
                PhoneNumber = phoneNumber,
                UserId = newApplicationUser?.Id ?? Guid.Empty.ToString(),
            };

            _context.UserDetails.Add(newUser);
            await _context.SaveChangesAsync();

            if (newApplicationUser == null)
            {
                throw new Exception("User creation failed.");
            }

            return newUser;
        }
    }
}
