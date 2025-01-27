using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using Escrow.Api.Infrastructure.Data;

namespace Escrow.Api.Infrastructure.Data;
public  class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        //// Ensure the configuration is loaded correctly (you can also configure it to point to your correct environment)
        //var configuration = new ConfigurationBuilder()
        //    .SetBasePath(Directory.GetCurrentDirectory())
        //    .AddJsonFile("appsettings.json") // Or use your specific settings file
        //    .Build();

        //var connectionString = configuration.GetConnectionString("Escrow.ApiDb");
        //optionsBuilder.UseNpgsql(connectionString); // Assuming you're using Npgsql, update accordingly

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
