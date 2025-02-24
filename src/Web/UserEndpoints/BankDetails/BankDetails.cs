namespace Escrow.Api.Web.Endpoints;

using Microsoft.AspNetCore.Authorization;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Features.Queries;
using Escrow.Api.Application.Features.Commands;




public class BankDetails : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization(policy => policy.RequireRole("User")) // Enable OpenIddict authorization
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
        
        var query = new GetBanksQuery { PageNumber = 1, PageSize = 10 };
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> CreateBankDetail(ISender sender,IJwtService jwtService, CreateBankCommand command)
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
