using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.AdminPanel;
using Escrow.Api.Domain.Entities.AMLPanel;
using Escrow.Api.Domain.Entities.Commissions;
using Escrow.Api.Domain.Entities.ContactUsPanel;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Entities.ContractReviews;
using Escrow.Api.Domain.Entities.Disputes;
using Escrow.Api.Domain.Entities.EmailTemplates;
using Escrow.Api.Domain.Entities.Notifications;
using Escrow.Api.Domain.Entities.Pages;
using Escrow.Api.Domain.Entities.RoleMenuPermissions;
using Escrow.Api.Domain.Entities.TeamMembers;
using Escrow.Api.Domain.Entities.Transactions;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<UserDetail> UserDetails { get; }
    DbSet<Page> Pages { get; }
    DbSet<BankDetail> BankDetails { get; }
    DbSet<ContractDetails> ContractDetails { get; }
    DbSet<MileStone> MileStones { get; }
    DbSet<AdminUser> AdminUsers { get; }
    DbSet<SellerBuyerInvitation> SellerBuyerInvitations { get; }
    DbSet<CommissionMaster> CommissionMasters { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Dispute> Disputes { get; }
    DbSet<DisputeMessage> DisputeMessages { get; }
    DbSet<AMLFlaggedTransaction> AMLFlaggedTransactions { get; }
    DbSet<AMLSettings> AMLSettings { get; }
    DbSet<AMLNotification> AMLNotifications { get; }
    DbSet<AMLTransactionVerification> AMLTransactionVerifications { get; }
    DbSet<ContactUs> ContactUs { get; }
    DbSet<EmailTemplate> EmailTemplates { get; }
    DbSet<TeamMember> TeamMembers { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<ContractReview> ContractReviews { get; }
    DbSet<ContractDetailsLog> ContractDetailsLogs { get; }
    DbSet<Menu> Menus { get; }
    DbSet<RoleMenuPermission> RoleMenuPermissions { get; }
    DbSet<Permission> Permissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync();
    Task<T> SaveChangesAndReturnAsync<T>(T entity) where T : class;
}
