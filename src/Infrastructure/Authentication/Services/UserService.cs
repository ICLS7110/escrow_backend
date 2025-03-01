using System;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.Authentication;
using Escrow.Api.Application.Authentication.Interfaces;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Events.UserPanel;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application;

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

        public async Task<UserDetail> FindOrCreateUserAsync(string phoneNumber)
        {
            var user = await _context.UserDetails.FindAsync(phoneNumber);
            if (user == null)
            {
                var entity = new UserDetail
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

        public async Task<UserDetail?> FindUserAsync(string phoneNumber)
        {
            var user = await _context.UserDetails.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);

            // Return null if the user is not found
            return user;
        }


        public async Task<UserDetail> CreateUserAsync(string phoneNumber)
        {
            var existingApplicationUser = await _userManager.Users.Where(u => u.PhoneNumber == phoneNumber).FirstOrDefaultAsync();   
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
                    throw new EscrowRecordCreationException($"User creation failed: {errors}");
                }

                // Assign "User" role
                var roleResult = await _userManager.AddToRoleAsync(newApplicationUser, "User");
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new EscrowRecordCreationException($"Role assignment failed: {roleErrors}");
                }
                newApplicationUser = await _userManager.FindByNameAsync(phoneNumber);
            }
            else
            {
                newApplicationUser = existingApplicationUser;
            }
            var newUser = new UserDetail
            {
                PhoneNumber = phoneNumber,
                UserId = newApplicationUser?.Id ?? Guid.Empty.ToString(),
            };

            _context.UserDetails.Add(newUser);
            await _context.SaveChangesAsync();

            if (newApplicationUser == null)
            {
                throw new EscrowRecordCreationException("User creation failed.");
            }

            return newUser;
        }
    }
}
