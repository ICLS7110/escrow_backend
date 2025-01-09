using System.Reflection;
using Escrow.Application.Common.Interfaces;
using Escrow.Domain.Entities;
using Escrow.Domain.Entities.UserPanel;
using Escrow.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        { 

        }
        DbSet<User> IApplicationDbContext.Users => Set<User>();
    }
}
