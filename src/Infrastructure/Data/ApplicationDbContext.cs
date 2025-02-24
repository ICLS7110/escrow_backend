namespace Escrow.Api.Infrastructure.Data;

using System.Reflection;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Enums;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;


public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

   
    public DbSet<User> UserDetails { get; set; }

    public DbSet<Bank> BankDetails => Set<Bank>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Entity<Bank>().HasQueryFilter(p => p.RecordState == RecordState.Active);
        builder.Entity<User>().HasQueryFilter(p => p.RecordState == RecordState.Active);

        
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
