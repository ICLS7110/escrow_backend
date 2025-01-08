using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Domain.Entities.UserPanel;

namespace Escrow.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
