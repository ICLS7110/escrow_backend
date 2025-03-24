using Escrow.Api.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Escrow.Api.Application.BankDetails.Queries;
using Escrow.Api.Application.BankDetails.Commands;
using Microsoft.AspNetCore.Authorization;
using Escrow.Api.Domain.Entities.UserPanel;
using Microsoft.AspNetCore.Mvc;
using Escrow.Api.Application.Common.Interfaces;

using Escrow.Api.Application;
using Escrow.Api.Infrastructure.Security;
using Escrow.Api.Application.Common.Models.BankDtos;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Web.Endpoints.BankDetails;

public class BankDetails : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User))) // Enable OpenIddict authorization
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) =>
        {
            // Optional: Add custom authorization logic if needed
            return await next(context);
        });
        
        userGroup.MapGet("/", GetBankDetails);
        userGroup.MapPost("/create", CreateBankDetail);
        userGroup.MapPost("/update", UpdateBankDetail);
        userGroup.MapDelete("/{id:int}", DeleteBankDetail);

    }

    [Authorize]
    public async Task<IResult> GetBankDetails(
        ISender sender,IJwtService jwtService)
    {
        
       var query = new GetBankDetailsQuery { Id = jwtService.GetUserId().ToInt(), PageNumber = 1, PageSize = 10 };
        var result = await sender.Send(query);
        return TypedResults.Ok(Result<PaginatedList<BankDetailDTO>>.Success(StatusCodes.Status200OK,"Success", result));
    }

    [Authorize]
    public async Task<IResult> CreateBankDetail(ISender sender, IJwtService jwtService, CreateBankDetailCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status201Created, "Bank details created successfully.", new { BankId = id }));
        //return TypedResults.Created($"/BankDetails/{id}", Result<int>.Success(StatusCodes.Status201Created, "Success.", id));
    }

    [Authorize]
    public async Task<IResult> UpdateBankDetail(ISender sender, IJwtService jwtService,  UpdateBankDetailCommand command)
    {      
        var result = await sender.Send(command);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent,"Bank details updated successfully.",new()));
    }

    [Authorize]
    public async Task<IResult> DeleteBankDetail(ISender sender, int id)
    {
        await sender.Send(new DeleteBankDetailCommand(id));
        return TypedResults.Ok( Result<object>.Success(StatusCodes.Status204NoContent, "Bank details Deleted successfully.", new())); 
    }
}
