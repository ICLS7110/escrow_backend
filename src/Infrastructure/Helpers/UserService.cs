using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Infrastructure.Helpers;
public class UserService : IUserService
{
    public class AppUser
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private readonly ApplicationDbContext _db;

    public UserService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await _db.Users.AnyAsync(u => u.Email == email);
    }

    public async Task CreateUserAsync(CreateUserDto dto)
    {
        var newUser = new UserDetail
        {
            EmailAddress = dto.Email,
            FullName = dto.FullName ?? dto.Email.Split('@')[0],
            Created = DateTime.UtcNow
        };

        _db.UserDetails.Add(newUser);
        await _db.SaveChangesAsync();
    }
}
