
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace Escrow.Api.Infrastructure.Configuration;
public class JwtService : IJwtService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public static IConfiguration? _config;
    public JwtService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _config = configuration;
    }

    public string GetUserId()
    {
        var userid = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userid ?? string.Empty;
    }
    public string GetJWT(string usereId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user.Id.ToString"),
            new Claim(ClaimTypes.Role,"User")
        };
        var secrectKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("teest"));
        var siginginCredentials = new SigningCredentials(secrectKey, SecurityAlgorithms.HmacSha256);
        var tokenOptions = new JwtSecurityToken(
            issuer: "teest",
            audience: "teest",
            claims: claims,
            expires: DateTime.Now.AddMinutes(5),
            signingCredentials: siginginCredentials
        );
        var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        return token;
    }

}
