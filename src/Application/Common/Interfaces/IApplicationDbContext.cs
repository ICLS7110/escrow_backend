using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }
    DbSet<UserDetail> UserDetails { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
