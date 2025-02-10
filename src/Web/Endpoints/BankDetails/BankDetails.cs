using Escrow.Api.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Escrow.Api.Application.BankDetails.Queries;
using Escrow.Api.Application.BankDetails.Commands;
using Microsoft.AspNetCore.Authorization;
using Escrow.Api.Domain.Entities.UserPanel;
using Microsoft.AspNetCore.Mvc;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Infrastructure.Configuration;
using Escrow.Api.Application.ResultHandler;
using Escrow.Api.Application;

namespace Escrow.Api.Web.Endpoints.BankDetails;

public class BankDetails : EndpointGroupBase
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

        userGroup.MapGet("/", GetBankDetails);        // Get all users  
        userGroup.MapGet("/{id:int}", GetBankDetails); // Get user by ID
        userGroup.MapPost("/", CreateBankDetail);
        userGroup.MapPut("/{id:int}", UpdateBankDetail);
        userGroup.MapDelete("/{id:int}", DeleteBankDetail);
    }

    [Authorize]
    public async Task<IResult> GetBankDetails(
        ISender sender,
        int? id,
        [AsParameters] GetBankDetailsQuery query)
    {
        query = new GetBankDetailsQuery { Id = id, PageNumber = query.PageNumber, PageSize = query.PageSize };
        var result = await sender.Send(query);
        return TypedResults.Ok(Result<PaginatedList<BankDetail>>.Success(result));
    }

    [Authorize]
    public async Task<IResult> CreateBankDetail(ISender sender,IJwtService jwtService, CreateBankDetailCommand command)
    {       
        var id = await sender.Send(command);
        return TypedResults.Ok(Result<int>.Success(id));
        //return TypedResults.Created($"/{nameof(BankDetails)}/{id}", id);
    }

    [Authorize]
    public async Task<IResult> UpdateBankDetail(ISender sender, IJwtService jwtService, int id, UpdateBankDetailCommand command)
    {
        if (id != command.Id)
        {
            throw new CustomValidationException("Invalid Request.");
        }       
        
        var result = await sender.Send(command);
        return TypedResults.Ok(Result<object>.Success(new { Message = "User details updated successfully." }));
    }

    [Authorize]
    public async Task<IResult> DeleteBankDetail(ISender sender, int id)
    {
        await sender.Send(new DeleteBankDetailCommand(id));
        return TypedResults.Ok( Result<object>.Success( new { Message = "Bank details Deleted successfully." } )); 
    }
}
