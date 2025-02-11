using Escrow.Api.Application.BankDetails.Commands;
using Escrow.Api.Application.BankDetails.Queries;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.ResultHandler;
using Escrow.Api.Domain.Entities.UserPanel;
using Microsoft.AspNetCore.Authorization;

namespace Escrow.Api.Web.AdminEndPoints.BankDetails;

public class BankDetails: EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization(policy => policy.RequireRole("Admin")) // Enable OpenIddict authorization
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) =>
        {
            // Optional: Add custom authorization logic if needed
            return await next(context);
        });

        userGroup.MapGet("/Admin", GetBankDetails);
        userGroup.MapGet("/Admin/{id:int}", GetBankDetails);
        userGroup.MapPost("/Admin/create", CreateBankDetail);
        userGroup.MapPost("Admin/update", UpdateBankDetail);
        userGroup.MapDelete("/Admin/{id:int}", DeleteBankDetail);

    }
    [Authorize]
    public async Task<IResult> GetBankDetails(
        ISender sender, IJwtService jwtService, int? id, [AsParameters] GetBankDetailsAdminQuery request)
    {

        var query = new GetBankDetailsAdminQuery { Id = id, PageNumber = request.PageNumber, PageSize = request.PageSize };
        var result = await sender.Send(query);
        return TypedResults.Ok(Result<PaginatedList<BankDetail>>.Success(result));
    }

    [Authorize]
    public async Task<IResult> CreateBankDetail(ISender sender, IJwtService jwtService, CreateBankDetailCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Ok(Result<int>.Success(id));
        //return TypedResults.Created($"/{nameof(BankDetails)}/{id}", id);
    }

    [Authorize]
    public async Task<IResult> UpdateBankDetail(ISender sender, IJwtService jwtService, UpdateBankDetailCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(Result<object>.Success(new { Message = "User details updated successfully." }));
    }

    [Authorize]
    public async Task<IResult> DeleteBankDetail(ISender sender, int id)
    {
        await sender.Send(new DeleteBankDetailCommand(id));
        return TypedResults.Ok(Result<object>.Success(new { Message = "Bank details Deleted successfully." }));
    }
}
