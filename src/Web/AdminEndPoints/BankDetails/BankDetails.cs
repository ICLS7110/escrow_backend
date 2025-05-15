using Escrow.Api.Application.BankDetails.Commands;
using Escrow.Api.Application.BankDetails.Queries;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Escrow.Api.Web.AdminEndPoints.BankDetails;

public class BankDetails: EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin))) // Enable OpenIddict authorization
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
    ISender sender,
    IJwtService jwtService,
    int? id,
    [AsParameters] GetBankDetailsAdminQuery request,
    IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var query = new GetBankDetailsAdminQuery { Id = id, PageNumber = request.PageNumber, PageSize = request.PageSize };
        var result = await sender.Send(query);

        var message = AppMessages.Get("BankDetailsRetrieved", language);
        return TypedResults.Ok(Result<PaginatedList<BankDetail>>.Success(StatusCodes.Status200OK, message, result));
    }

    [Authorize]
    public async Task<IResult> CreateBankDetail(ISender sender,IJwtService jwtService,CreateBankDetailCommand command,IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var id = await sender.Send(command);
        var message = AppMessages.Get("BankDetailCreated", language);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status201Created, message, new { BankId = id }));
    }

    [Authorize]
    public async Task<IResult> UpdateBankDetail(ISender sender,IJwtService jwtService,UpdateBankDetailCommand command,IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(command);
        var message = AppMessages.Get("BankDetailUpdated", language);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent, message, new()));
    }

    [Authorize]
    public async Task<IResult> DeleteBankDetail(ISender sender,int id,IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        await sender.Send(new DeleteBankDetailCommand(id));
        var message = AppMessages.Get("BankDetailDeleted", language);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent, message, new()));
    }


    //[Authorize]
    //public async Task<IResult> GetBankDetails(
    //    ISender sender, IJwtService jwtService, int? id, [AsParameters] GetBankDetailsAdminQuery request)
    //{

    //    var query = new GetBankDetailsAdminQuery { Id = id, PageNumber = request.PageNumber, PageSize = request.PageSize };
    //    var result = await sender.Send(query);
    //    return TypedResults.Ok(Result<PaginatedList<BankDetail>>.Success(StatusCodes.Status200OK,"Success", result));
    //}


    //[Authorize]
    //public async Task<IResult> CreateBankDetail(ISender sender, IJwtService jwtService, CreateBankDetailCommand command)
    //{
    //    var id = await sender.Send(command);
    //    return TypedResults.Ok(Result<object>.Success(StatusCodes.Status201Created, "Bank details created successfully.", new { BankId = id }));
    //    //return TypedResults.Created($"/BankDetails/{id}", Result<int>.Success(StatusCodes.Status201Created, "Success.", id));
    //}



    //[Authorize]
    //public async Task<IResult> UpdateBankDetail(ISender sender, IJwtService jwtService, UpdateBankDetailCommand command)
    //{
    //    var result = await sender.Send(command);
    //    return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent, "Bank details updated successfully.", new()));
    //}

    //[Authorize]
    //public async Task<IResult> DeleteBankDetail(ISender sender, int id)
    //{
    //    await sender.Send(new DeleteBankDetailCommand(id));
    //    return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent,"Bank details Deleted successfully.", new()));
    //}
}
