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

namespace Escrow.Api.Infrastructure.Authentication.Services
{
    public class UserService : IUserService
    {
        private readonly IApplicationDbContext _context;

        public UserService(IApplicationDbContext context)
        {
            _context = context;
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
            var newUser = new UserDetail
            {
                PhoneNumber = phoneNumber,
                UserId = Guid.NewGuid().ToString()  // Generate a new unique UserId (GUID)
            };

            _context.UserDetails.Add(newUser);
            await _context.SaveChangesAsync();

            if (newUser == null)
            {
                throw new InvalidOperationException("User creation failed.");
            }

            return newUser;
        }
    }
}
