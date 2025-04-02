using System.Reflection;
using System.Reflection.Emit;
using Amazon.Auth.AccessControlPolicy;
using Escrow.Api.Application.BankDetails.Commands;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.AdminPanel;
using Escrow.Api.Domain.Entities.AMLPanel;
using Escrow.Api.Domain.Entities.Commissions;
using Escrow.Api.Domain.Entities.ContactUsPanel;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Entities.Disputes;
using Escrow.Api.Domain.Entities.EmailTemplates;
using Escrow.Api.Domain.Entities.Notifications;
using Escrow.Api.Domain.Entities.Pages;
using Escrow.Api.Domain.Entities.TeamMembers;
using Escrow.Api.Domain.Entities.Transactions;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using Escrow.Api.Infrastructure.Configuration;
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
        //if (!optionsBuilder.IsConfigured)
        //{
        //    //optionsBuilder.UseNpgsql("Escrow.ApiDb"); // Or another provider, e.g., SQL Server
        //    optionsBuilder.UseNpgsql("Host=103.119.170.253;Database=escrow;Username=root;Password=root@123;Persist Security Info=True");
        //    //
        //}
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }


    public DbSet<UserDetail> UserDetails { get; set; }
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<BankDetail> BankDetails => Set<BankDetail>();
    public DbSet<ContractDetails> ContractDetails => Set<ContractDetails>();
    public DbSet<MileStone> MileStones => Set<MileStone>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<SellerBuyerInvitation> SellerBuyerInvitations => Set<SellerBuyerInvitation>();
    public DbSet<CommissionMaster> CommissionMasters => Set<CommissionMaster>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AMLFlaggedTransaction> AMLFlaggedTransactions => Set<AMLFlaggedTransaction>();
    public DbSet<AMLNotification> AMLNotifications => Set<AMLNotification>();
    public DbSet<AMLSettings> AMLSettings => Set<AMLSettings>();
    public DbSet<AMLTransactionVerification> AMLTransactionVerifications => Set<AMLTransactionVerification>();
    public DbSet<Dispute> Disputes => Set<Dispute>();
    public DbSet<DisputeMessage> DisputeMessages => Set<DisputeMessage>();
    public DbSet<ContactUs> ContactUs => Set<ContactUs>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Notification> Notifications => Set<Notification>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Map ApplicationUser to UserDetails table
        //builder.Entity<ApplicationUser>().ToTable("UserDetail");

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Entity<BankDetail>().HasQueryFilter(p => p.RecordState == RecordState.Active);
        builder.Entity<UserDetail>().HasQueryFilter(p => p.RecordState == RecordState.Active);
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
