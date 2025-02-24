namespace Escrow.Api.Web.AdminEndPoints.BankDetails;

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Features.Commands;
using Escrow.Api.Application.Features.Queries;
using Microsoft.AspNetCore.Authorization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


public class BankDetails: EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization(policy => policy.RequireRole("Admin")) 
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) =>
        {
          
            return await next(context);
        });

        userGroup.MapGet("/Admin", GetBankDetails);
        userGroup.MapGet("/Admin/{id:int}", GetBankDetails);
        userGroup.MapPost("/Admin/create", CreateBankDetail);
        userGroup.MapPost("Admin/update", UpdateBankDetail);
        userGroup.MapDelete("/Admin/{id:int}", DeleteBankDetail);

    }
    [Authorize]
    public async Task<IResult> GetBankDetails(ISender sender, [AsParameters] GetBankByIdQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> CreateBankDetail(ISender sender, IJwtService jwtService, CreateBankCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> UpdateBankDetail(ISender sender, IJwtService jwtService, UpdateBankCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> DeleteBankDetail(ISender sender, DeleteBankCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(result);
    }
}
