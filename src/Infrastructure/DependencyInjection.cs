using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Services;
using Escrow.Api.Domain.Constants;
using Escrow.Api.Domain.Interfaces;
using Escrow.Api.Infrastructure.Configuration;
using Escrow.Api.Infrastructure.Data;
using Escrow.Api.Infrastructure.Data.Interceptors;
using Escrow.Api.Infrastructure.Identity;
using Escrow.Api.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        // Get the connection string from appsettings.json or environment variables
        var connectionString = configuration.GetConnectionString("Escrow");
        Guard.Against.Null(connectionString, message: "Connection string 'Escrow' not found.");

        // Register database context and interceptors
        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        // Configure TwilioSettings (from appsettings.json or environment variables)
        builder.Services.Configure<TwilioSettings>(configuration.GetSection("TwilioSettings"));

        // Register application services
        builder.Services.AddScoped<IOtpService, TwilioOtpService>();
        builder.Services.AddScoped<IOtpValidationService, PhoneNumberValidationService>();
        builder.Services.AddScoped<IOtpManagerService, OtpManagerService>();

        // Register the DbContext with Npgsql and interceptors
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
        });

        // Register IApplicationDbContext and ApplicationDbContext initializer
        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        // Configure authentication with Bearer token scheme
        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        // Configure authorization policies
        builder.Services.AddAuthorizationBuilder();
        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        // Register Identity services
        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        // Register other services
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();
    }
}
