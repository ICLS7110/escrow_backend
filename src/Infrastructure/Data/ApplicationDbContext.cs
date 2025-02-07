using System.Reflection;
using System.Reflection.Emit;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace Escrow.Api.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            //optionsBuilder.UseNpgsql("Escrow.ApiDb"); // Or another provider, e.g., SQL Server
            optionsBuilder.UseNpgsql("Host=103.189.173.7;Database=escrow;Username=root;Password=root@123;Persist Security Info=True");
            //
        }
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    //public DbSet<UserDetail> UserDetails => Set<UserDetail>();
    public DbSet<UserDetail> UserDetails { get; set; }

    public DbSet<BankDetail> BankDetails => Set<BankDetail>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Register the OpenIddict models
        //builder.UseOpenIddict();
    }

    public Task<int> SaveChangesAsync()
    {
        return base.SaveChangesAsync();
    }

    public async Task<T> SaveChangesAndReturnAsync<T>(T entity) where T : class
    {
        var entry = Entry(entity);

        // Check if the entity is being added or modified
        if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
        {
            await SaveChangesAsync();
        }

        return entity;
    }
}
