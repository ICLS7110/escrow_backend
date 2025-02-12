using Escrow.Api.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Escrow.Api.Application.UserPanel.Queries.GetUsers;
using Escrow.Api.Application.UserPanel.Commands.CreateUser;
using Escrow.Api.Application.UserPanel.Commands.UpdateUser;
using Escrow.Api.Application.UserPanel.Commands.DeleteUser;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;
using Escrow.Api.Domain.Entities.UserPanel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.AspNetCore.Mvc;
using Escrow.Api.Application.ResultHandler;
using Escrow.Api.Application;
using Escrow.Api.Application.Common.Interfaces;

namespace Escrow.Api.Web.Endpoints.UserPanel;

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
        var query = new GetUserDetailsQuery { Id = Convert.ToInt32(jwtService.GetUserId()), PageNumber = 1, PageSize = 1};
        var result = await sender.Send(query);
        return TypedResults.Ok(Result<PaginatedList<UserDetailDto>>.Success(result));
    }

    //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Authorize]
    public async Task<IResult> CreateUser(ISender sender, CreateUserCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Ok(Result<int>.Success(id));
    }

    //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Authorize]
    public async Task<IResult> UpdateUserDetail(ISender sender, UpdateUserCommand command)
    {        
         await sender.Send(command);
         return TypedResults.Ok(Result<object>.Success(new { Message = "User details updated successfully." }));        
    }

    //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Authorize]
    public async Task<IResult> DeleteUser(ISender sender, int id)
    {
        await sender.Send(new DeleteUserCommand(id));
        return TypedResults.Ok(Result<object>.Success(new { Message = "User details Deleted successfully." }));   
    }
}
