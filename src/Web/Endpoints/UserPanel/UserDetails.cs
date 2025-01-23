using Escrow.Api.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Escrow.Api.Application.UserPanel.Queries.GetUsers;
using Escrow.Api.Application.UserPanel.Commands.CreateUser;
using Escrow.Api.Application.UserPanel.Commands.UpdateUser;
using Escrow.Api.Application.UserPanel.Commands.DeleteUser;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;

namespace Escrow.Api.Web.Endpoints.UserPanel;

public class UserDetails : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()  // Enable OpenIddict authorization
            .WithTags("User Management")
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

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<Ok<PaginatedList<UserDetailDto>>> GetUserDetails(ISender sender, [AsParameters] GetUserDetailsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<Created<int>> CreateUser(ISender sender, CreateUserCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(UserDetails)}/{id}", id);
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<Results<NoContent, BadRequest>> UpdateUserDetail(ISender sender, int id, UpdateUserCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<NoContent> DeleteUser(ISender sender, int id)
    {
        await sender.Send(new DeleteUserCommand(id));
        return TypedResults.NoContent();
    }
}
