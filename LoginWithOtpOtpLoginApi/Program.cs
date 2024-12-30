using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OtpLoginApi.Data;
using OtpLoginApi.Models;
using OtpLoginApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework and OpenIddict
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseOpenIddict(); // For OpenIddict integration
});

// Add Twilio settings and PhoneNumberValidationService
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));
builder.Services.AddSingleton<PhoneNumberValidationService>();

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure OpenIddict for token management
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.AllowPasswordFlow();
        options.AllowCustomFlow("otp");

        options.SetTokenEndpointUris("/connect/token");
        options.AddEphemeralEncryptionKey()
               .AddEphemeralSigningKey();
        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// Configure IIS server settings and form options
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue;
});

// Set up CORS for frontend-to-backend communication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.WithOrigins("https://localhost:7017")  // Add your frontend URL here
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Configure form options for large file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = int.MaxValue;
});

// Add controllers and Swagger for API documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger UI setup for development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable routing, CORS, authentication, and authorization
app.UseRouting();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map controllers for endpoint handling
app.MapControllers();

app.Run();
