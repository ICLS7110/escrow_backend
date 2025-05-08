using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Constants;
using Escrow.Api.Application.Authentication.Interfaces;
using Escrow.Api.Infrastructure.Configuration;
using Escrow.Api.Infrastructure.Data;
using Escrow.Api.Infrastructure.Data.Interceptors;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Escrow.Api.Infrastructure.Authentication.Services;
using Escrow.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Escrow.Api.Infrastructure.OptionConfiguration;
using Microsoft.AspNetCore.Http.Features;
using Amazon.S3;
using Amazon;
using Escrow.Api.Infrastructure.Helpers.Notifications;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        // Get the connection string
        var connectionString = builder.Configuration.GetConnectionString("Escrow");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Escrow' not found.");
        }

        // Register database interceptors
        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        // Configure TwilioSettings (from appsettings.json or environment variables)
        var twilioSection = builder.Configuration.GetSection("TwilioSettings");
        if (!twilioSection.Exists())
        {
            throw new InvalidOperationException("TwilioSettings section not found.");
        }
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IOtpService, SimpleOtpService>(); // Replace TwilioOtpService with SimpleOtpService
        builder.Services.AddScoped<IOtpValidationService, PhoneNumberValidationService>();
        builder.Services.AddScoped<IOtpManagerService, OtpManagerService>();
        builder.Services.AddScoped<OtpManagerService>();

        // Register DbContext with Npgsql and interceptors
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
        });

        builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));
        builder.Services.Configure<AESSettings>(builder.Configuration.GetSection("AESSettings"));
        builder.Services.Configure<AWSS3>(builder.Configuration.GetSection("AWSS3"));

        // Register IApplicationDbContext and initializer
        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        // Configure authentication and bearer tokens
        builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
            .AddBearerToken(IdentityConstants.BearerScheme);

        // Configure authorization policies
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator));
        });

        // Register Identity services
        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddMemoryCache();

        // Register other services
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, Escrow.Api.Infrastructure.Authentication.Services.IdentityEmailSender>();
        builder.Services.AddSingleton<IRsaHelper, RsaHelper>();
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        //builder.Services.AddSingleton<INotificationService, FirebaseNotificationService>();
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddSingleton<IAESService, AESService>();
        builder.Services.AddScoped<IFileService, FileService>();

        builder.Services.AddScoped<INotificationService, FirebaseNotificationService>(); // ✅

    }


}
