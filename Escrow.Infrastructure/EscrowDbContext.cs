using Escrow.Domain.UserPanel;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Infrastructure
{
    public class EscrowDbContext : IdentityDbContext<ApplicationUser>
    {
        public EscrowDbContext(DbContextOptions<EscrowDbContext> options) : base(options) { }
        public DbSet<UserDetails> UserDetails { get; set; }
    }
}
