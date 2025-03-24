using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.AdminPanel;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Entities.Policy;

namespace Escrow.Api.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<UserDetail> UserDetails { get; }
    DbSet<BankDetail> BankDetails { get; }
    DbSet<ContractDetails> ContractDetails { get; }
    DbSet<MileStone> MileStones { get; }
    DbSet<AdminUser> AdminUsers { get; }
    DbSet<SellerBuyerInvitation> SellerBuyerInvitations { get; }
    DbSet<Policy> Policies { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync();
    Task<T> SaveChangesAndReturnAsync<T>(T entity) where T : class;
}
