using System;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.Authentication;
using Escrow.Api.Domain.Interfaces;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Escrow.Api.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User> FindOrCreateUserAsync(string phoneNumber)
        {
            var user = await _userManager.FindByNameAsync(phoneNumber);
            if (user == null)
            {
                // Creating a new ApplicationUser (not IdentityUser)
                user = new ApplicationUser { UserName = phoneNumber, PhoneNumber = phoneNumber };
                var result = await _userManager.CreateAsync(user);
                
                // Check if creation was successful
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException("User creation failed.");
                }
            }

            return new User { Id = user.Id, PhoneNumber = user.PhoneNumber };
        }

        public async Task<User> FindUserAsync(string phoneNumber)
        {
            var user = await _userManager.FindByNameAsync(phoneNumber);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            return new User { Id = user.Id, PhoneNumber = user.PhoneNumber };
        }
    }
}
