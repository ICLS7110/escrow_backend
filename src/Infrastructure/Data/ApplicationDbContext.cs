using System.Reflection;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
            optionsBuilder.UseNpgsql("Host=localhost;Database=Escrow.ApiDb;Username=postgres;Password=1234;Port=5433");
                       
        }
    }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<UserDetail> UserDetails => Set<UserDetail>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
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
