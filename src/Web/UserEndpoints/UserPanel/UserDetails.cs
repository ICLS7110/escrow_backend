namespace Escrow.Api.Web.Endpoints.UserPanel; 

using Escrow.Api.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Handler;
using Escrow.Api.Application.Features.Commands;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public class UserDetails : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization() // Enable OpenIddict authorization
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) =>
        {
            // Optional: Add custom authorization logic if needed
            return await next(context);
        });
        
                // Get all users  
        userGroup.MapGet("/", GetUserDetails).RequireAuthorization(policy => policy.RequireRole("User")); // Get user by ID
        userGroup.MapPost("/update", UpdateUserDetail).RequireAuthorization(policy => policy.RequireRole("User"));
        
    }

    [Authorize]
    public async Task<IResult> GetUserDetails(ISender sender,IJwtService jwtService )
    {
        var query = new GetUserDetailsQuery { Id = int.Parse(jwtService.GetUserId()), PageNumber = 1, PageSize = 1};
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> CreateUser(ISender sender, CreateUserCommand command)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> UpdateUserDetail(ISender sender, UpdateUserCommand command)
    {        
         var result = await sender.Send(command);
         return TypedResults.Ok(result);        
    }

    [Authorize]
    public async Task<IResult> DeleteUser(ISender sender, DeleteUserCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }
}
