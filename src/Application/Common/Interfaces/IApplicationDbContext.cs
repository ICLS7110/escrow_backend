using Escrow.Api.Domain.Entities;


namespace Escrow.Api.Application.Common.Interfaces;

public interface IApplicationDbContext
{   
    DbSet<User> UserDetails { get; }
    DbSet<Bank> BankDetails { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync();
    Task<T> SaveChangesAndReturnAsync<T>(T entity) where T : class;
}
