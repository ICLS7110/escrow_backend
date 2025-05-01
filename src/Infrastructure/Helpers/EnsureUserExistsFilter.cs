using Escrow.Api.Application.Authentication.Interfaces;
using Escrow.Api.Application.Common.Models;
using Microsoft.AspNetCore.Http;

public class EnsureUserExistsFilter : IEndpointFilter
{
    private readonly IUserService _userService;

    public EnsureUserExistsFilter(IUserService userService)
    {
        _userService = userService;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var user = httpContext.User;

        var email = user?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var name = user?.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        var picture = user?.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.Unauthorized();
        }

        var exists = await _userService.UserExistsAsync(email);
        if (!exists)
        {
            await _userService.CreateUserAsync(new CreateUserDto
            {
                Email = email,
                FullName = name,
                ProfilePictureUrl = picture
            });
        }

        return await next(context);
    }
}
