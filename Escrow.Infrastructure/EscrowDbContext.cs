using Escrow.Domain.Entities.UserPanel;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Infrastructure
{
    public class EscrowDbContext : IdentityDbContext<User>
    {
        public EscrowDbContext(DbContextOptions<EscrowDbContext> options) : base(options) { }
        public DbSet<User> UserDetails { get; set; }
    }
}
