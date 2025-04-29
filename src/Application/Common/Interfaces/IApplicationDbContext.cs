using System.Globalization;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.Commissions;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Entities.ContractReviews;
using Escrow.Api.Domain.Entities.Notifications;
using Escrow.Api.Domain.Entities.TeamMembers;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using MediatR;

namespace Escrow.Api.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<UserDetail> UserDetails { get; }
    DbSet<BankDetail> BankDetails { get; }
    DbSet<ContractDetails> ContractDetails { get; }
    DbSet<MileStone> MileStones { get; }
    DbSet<SellerBuyerInvitation> SellerBuyerInvitations { get; }
    DbSet<ContractReview> ContractReviews { get; }
    DbSet<ContractDetailsLog> ContractDetailsLogs { get; }
    DbSet<CommissionMaster> CommissionMasters { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<TeamMember> TeamMembers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync();
    Task<T> SaveChangesAndReturnAsync<T>(T entity) where T : class;
}
