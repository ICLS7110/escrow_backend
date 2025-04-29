
using System.Text;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Infrastructure.Data;
using Escrow.Api.Infrastructure.Helpers;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;




var builder = WebApplication.CreateBuilder(args);
ConfigurationHelper.InitializeConfig(builder.Configuration);


// Add services to the container
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure OpenIddict for token management
/*
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        // Register the OpenIddict server handlers and options
        options.SetAuthorizationEndpointUris("connect/authorize")
        .SetTokenEndpointUris("/connect/token");

        options.AllowAuthorizationCodeFlow();
        options.AllowPasswordFlow();

        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        options.AddEphemeralEncryptionKey()
               .AddEphemeralSigningKey();

        // Register the ASP.NET Core host and configure token validation
        options.UseAspNetCore()
               //.EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough();

    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();

        // Configure your authority(issuer) URL.
        //options.SetIssuer("https://your-authority-url.com/");

        // Enable token validation against your application.
        //options.AddAudiences("backend-api");
    });*/

/*builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});*/
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var response = "Access Denied. You do not have permission to access this resource.";
            await context.Response.WriteAsJsonAsync(Result<string>.Failure(StatusCodes.Status403Forbidden, response));
        }
    };
    var issuerSigningKey = builder.Configuration["Jwt:IssuerSigningKey"]
        ?? throw new InvalidOperationException("Jwt:IssuerSigningKey is missing in the configuration.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = ConfigurationHelper.JwtValidIssuer,
        ValidAudience = ConfigurationHelper.JwtValidAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddAntiforgery();
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();
builder.Services.AddControllers();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();
app.UseCors("AllowAll");
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
    //await AdminSeedService.EnsureAdminUserExists(services);
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHealthChecks("/health");
//app.UseHttpsRedirection();
app.UseStaticFiles();
//app.UseAuthentication();
//app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    // Enable Swagger in Development environment
    app.UseSwaggerUi(settings =>
    {
        settings.Path = "/api";
        settings.DocumentPath = "/api/specification.json";
    });
}
else
{
    app.UseHsts();
}

//app.MapStaticAssets();
//app.UseExceptionHandler(options => { });
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAntiforgery();
app.Map("/", () => Results.Redirect("/api"));

app.MapEndpoints();
Log.Information("Starting web application");
app.Run();

public partial class Program { }
