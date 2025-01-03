using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EscrowAPI.Data;
using Volo.Abp.DependencyInjection;

namespace EscrowAPI.EntityFrameworkCore;

public class EntityFrameworkCoreEscrowAPIDbSchemaMigrator
    : IEscrowAPIDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreEscrowAPIDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the EscrowAPIDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<EscrowAPIDbContext>()
            .Database
            .MigrateAsync();
    }
}
