
using Escrow.Api.Infrastructure.Data;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Infrastructure.Helpers;




var builder = WebApplication.CreateBuilder(args);
ConfigurationHelper.InitializeConfig(builder.Configuration);

builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityDbConnection"));
    options.UseOpenIddict();
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<IdentityDbContext>(); 
    })
    .AddServer(options =>
    {
        options.AllowPasswordFlow()
               .AllowRefreshTokenFlow();

        options.SetTokenEndpointUris("/connect/token");

        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough();
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

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

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

app.Run();

public partial class Program { }
