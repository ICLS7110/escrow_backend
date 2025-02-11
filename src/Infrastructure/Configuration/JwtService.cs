using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Infrastructure.Configuration;
public class JwtService : IJwtService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId()
    {
        var userid= _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userid ?? string.Empty;
    }
    
}
