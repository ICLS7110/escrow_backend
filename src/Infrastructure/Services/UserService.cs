using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.Authentication;
using Escrow.Api.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Escrow.Api.Infrastructure.Services;
public class UserService : IUserService
{
    private readonly UserManager<IdentityUser> _userManager;

    public UserService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<User> FindOrCreateUserAsync(string phoneNumber)
    {
        var user = await _userManager.FindByNameAsync(phoneNumber);
        if (user == null)
        {
            user = new IdentityUser { UserName = phoneNumber, PhoneNumber = phoneNumber };
            await _userManager.CreateAsync(user);
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
