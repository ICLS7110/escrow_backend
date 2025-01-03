using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace EscrowAPI.Data;

/* This is used if database provider does't define
 * IEscrowAPIDbSchemaMigrator implementation.
 */
public class NullEscrowAPIDbSchemaMigrator : IEscrowAPIDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
