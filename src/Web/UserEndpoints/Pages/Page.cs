using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Policies.Commands;
using Escrow.Api.Application.Policies.Queries;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Escrow.Api.Web.UserEndpoints.Pages;

public class Pages : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var adminGroup = app.MapGroup(this).AllowAnonymous()
           
            .WithOpenApi();

        adminGroup.MapGet("/", GetAllPages);
        adminGroup.MapPut("/update", UpdatePages);
    }

    /// <summary>
    /// Retrieves all pages or a specific page by ID.
    /// Supports pagination.
    /// </summary>
    public async Task<IResult> GetAllPages(ISender sender, int? id, int pageNumber = 1, int pageSize = 10)
    {
        var result = await sender.Send(new GetAllPagesQuery
        {
            Id = id,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

        //if (result == null || !result.Items.Any())
        //{
        //    return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "No pages found."));
        //}

        return TypedResults.Ok(Result<PaginatedList<PagesDTO>>.Success(StatusCodes.Status200OK, "Pages retrieved successfully.", result));
    }

    public async Task<IResult> UpdatePages(ISender sender, UpdatePagesCommand command)
    {
        var result = await sender.Send(command);

        // 🔹 Return 400 Bad Request if no pages are provided
        if (result.Status == StatusCodes.Status400BadRequest)
        {
            return TypedResults.BadRequest(result);
        }

        // 🔹 Return 404 Not Found if no matching pages exist
        if (result.Status == StatusCodes.Status404NotFound)
        {
            return TypedResults.NotFound(result);
        }

        // 🔹 Return 200 OK if Update is successful
        return TypedResults.Ok(result);
    }
}

