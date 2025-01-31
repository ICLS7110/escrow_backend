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

namespace Escrow.Api.Web.Endpoints.UserPanel;

public class UserDetails : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()  // Enable OpenIddict authorization
            //.WithTags("User Management")
            .WithOpenApi()
            .AddEndpointFilter(async (context, next) =>
            {
                // Optional: Add custom authorization logic if needed
                return await next(context);
            })
            .MapGet(GetUserDetails)
            .MapPost(CreateUser)
            .MapPut(UpdateUserDetail, "{id}")
            .MapDelete(DeleteUser, "{id}");
    }

    [Authorize]
    public async Task<Ok<List<UserDetail>>> GetUserDetails(ISender sender, [AsParameters] GetUserDetailsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Authorize]
    public async Task<Created<int>> CreateUser(ISender sender, CreateUserCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(UserDetails)}/{id}", id);
    }

    //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Authorize]
    public async Task<Results<IResult, BadRequest>> UpdateUserDetail(ISender sender, int id, UpdateUserCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        try
        {
            await sender.Send(command);
            return TypedResults.Ok(new { Message = "User details updated successfully.", UserId = command.Id });
        }
        catch (Exception ex)
        {
            // Handle unexpected errors
            return TypedResults.Problem($"An error occurred: {ex.Message}");
        }
    }

    //[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Authorize]
    public async Task<IResult> DeleteUser(ISender sender, int id)
    {
        try
        {
            await sender.Send(new DeleteUserCommand(id));
            return TypedResults.NoContent();
        }
        catch (Exception ex)
        {
            // Handle unexpected errors
            return TypedResults.Problem($"An error occurred: {ex.Message}");
        }       
    }
}
