using System.Threading.Tasks;

namespace EscrowAPI.Data;

public interface IEscrowAPIDbSchemaMigrator
{
    Task MigrateAsync();
}
