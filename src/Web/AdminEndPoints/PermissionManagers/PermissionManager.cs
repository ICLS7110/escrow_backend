using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.PermissionManagers.Commands;
using Escrow.Api.Application.PermissionManagers.Queries;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web.AdminEndPoints.PermissionManagers;

public class PermissionManager : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithOpenApi();

        group.MapGet("/roles", GetRoles);
        group.MapGet("/menus", GetMenus);
        group.MapGet("/permissions", GetPermissions);
        group.MapGet("/role-permissions", GetRolePermissions);
        group.MapPost("/assign-permissions", AssignPermissionsToRole);
    }

    [Authorize]
    [HttpGet("roles")]
    public async Task<IResult> GetRoles([FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetAdminRolesQuery());

        if (result.Data != null)
        {
            return TypedResults.Ok(result.Data);
        }

        return TypedResults.BadRequest(new { message = result.Message });
    }


    [Authorize]
    public async Task<IResult> GetMenus(ISender sender)
    {
        var result = await sender.Send(new GetMenusQuery());
        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> GetPermissions(ISender sender)
    {
        var result = await sender.Send(new GetPermissionsQuery());
        return TypedResults.Ok(result);
    }






    [Authorize]
    public async Task<IResult> GetRolePermissions(ISender sender, int? userId, int pageNumber = 1, int pageSize = 10)
    {
        // Send the query with parameters to the handler
        var result = await sender.Send(new GetRoleMenuPermissionsQuery
        {
            UserId = userId,   // Pass userId into the query
            PageNumber = pageNumber, // Pass pageNumber into the query
            PageSize = pageSize  // Pass pageSize into the query
        });

        // Check if the result is not null and return the result
        if (result != null)
        {
            return TypedResults.Ok(result);
        }

        // Return empty result if no data is found
        return TypedResults.Ok(new { message = "No data found" });
    }



    [Authorize]
    [HttpPost("assign-permissions")]
    public async Task<IResult> AssignPermissionsToRole(ISender sender, [FromBody] AssignPermissionsCommand command)
    {
        var result = await sender.Send(command);
        if (result == null)
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Failed to assign permissions."));
        return TypedResults.Ok(result);

    }



    //[Authorize]
    //public async Task<IResult> AssignPermissionsToRole(ISender sender, [FromBody] AssignPermissionsCommand command)
    //{
    //    var result = await sender.Send(command);
    //    return TypedResults.Ok(result);
    //}
}
