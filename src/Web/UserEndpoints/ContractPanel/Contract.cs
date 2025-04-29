using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.ContractPanel.ContractCommands;
using Escrow.Api.Application.ContractPanel.ContractQueries;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.ContractPanel.MilestoneQueries;
using Escrow.Api.Domain.Enums;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application;

namespace Escrow.Api.Web.Endpoints.ContractPanel;

public class Contract : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization()
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) => await next(context));

        userGroup.MapGet("/", GetContractDetails).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User), nameof(Roles.Admin)));
        userGroup.MapGet("/allcontracts", GetAllContractDetails).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User), nameof(Roles.Admin)));
        userGroup.MapPost("/", CreateContractDetails).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User), nameof(Roles.Admin)));
        userGroup.MapPost("/update", UpdateContractDetail).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User), nameof(Roles.Admin)));
        userGroup.MapPost("/update-status", UpdateContractStatus).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User), nameof(Roles.Admin)));
        userGroup.MapGet("/buyersellercontracts", GetContractByBuyerSellerDetails).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User), nameof(Roles.Admin)));
        userGroup.MapGet("/{id:int}", GetContractById).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin)));
        userGroup.MapPost("/change-status", ChangeContractStatus).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User), nameof(Roles.Admin)));
        userGroup.MapGet("/list", GetContracts).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User), nameof(Roles.Admin)));
        userGroup.MapPost("/modify", ModifyContract).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User), nameof(Roles.Admin)));

    }

    private bool IsAdmin(IHttpContextAccessor httpContextAccessor) =>
        httpContextAccessor.HttpContext?.User.IsInRole(nameof(Roles.Admin)) ?? false;

    [Authorize]
    public async Task<IResult> GetContractDetails(ISender sender, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, int? contractId)
    {
        var userId = jwtService.GetUserId().ToInt();
        var query = new GetContractForUserQuery
        {
            Id = IsAdmin(httpContextAccessor) ? null : userId,
            ContractId = contractId,
            PageNumber = 1,
            PageSize = 10
        };
        var result = await sender.Send(query);
        return TypedResults.Ok(Result<PaginatedList<ContractDTO>>.Success(StatusCodes.Status200OK, "Success", result));
    }

    [Authorize]
    public async Task<IResult> GetAllContractDetails(ISender sender, ContractStatus? status, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
    {
        var userId = jwtService.GetUserId().ToInt();

        var query = new GetContractForUserQuery
        {
            //Id = IsAdmin(httpContextAccessor) ? null : userId,
            Status = status,
            PageNumber = 1,
            PageSize = 10
        };
        var result = await sender.Send(query);
        return TypedResults.Ok(Result<PaginatedList<ContractDTO>>.Success(StatusCodes.Status200OK, "Success", result));
    }

    [Authorize]
    public async Task<IResult> GetContractByBuyerSellerDetails(ISender sender, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, int? buyerId, int? sellerId, int? contractId)
    {
        var userId = jwtService.GetUserId().ToInt();
        var query = new GetContractForBuyerQuery
        {
            Id = IsAdmin(httpContextAccessor) ? null : userId,
            BuyerId = buyerId,
            SellerId = sellerId,
            ContractId = contractId,
            PageNumber = 1,
            PageSize = 10
        };
        var result = await sender.Send(query);
        return TypedResults.Ok(Result<PaginatedList<ContractDetailsDTO>>.Success(StatusCodes.Status200OK, "Success", result));
    }

    [Authorize]
    public async Task<IResult> UpdateContractStatus(ISender sender, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, UpdateContractStatusCommand command)
    {

        var result = await sender.Send(command);
        if (!result)
            return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "Contract not found or update failed."));

        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Contract status updated successfully.", new()));
    }

    [Authorize]
    public async Task<IResult> UpdateContractDetail(ISender sender, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, EditContractDetailCommand command)
    {

        var result = await sender.Send(command);
        return TypedResults.Ok(
    Result<object>.Success(
        StatusCodes.Status204NoContent,
        "Contract details updated successfully.",
        new { contractId = result.Data }
    )
);

    }

    [Authorize]
    public async Task<IResult> CreateContractDetails(ISender sender, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, CreateContractDetailCommand command)
    {
        try
        {
            var id = await sender.Send(command);
            return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent, "Contract Created Successfully.", new { contractId = id }));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [Authorize]
    public async Task<IResult> GetContractById(ISender sender, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, int id)
    {
        var result = await sender.Send(new GetContractByIdQuery(id));
        return result.Status == 1 ? TypedResults.NotFound(result) : TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> ChangeContractStatus(ISender sender, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, AdminChangeContractStatusCommand command)
    {
        if (!IsAdmin(httpContextAccessor))
            return TypedResults.Forbid();

        var result = await sender.Send(command);

        if (result.Status != StatusCodes.Status200OK)
            return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "Contract not found or update failed."));

        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, result.Message, new()));
    }

    [Authorize]
    public async Task<IResult> GetContracts(ISender sender, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, ContractStatus? status, string? searchKeyword, int? priceFilter, bool? isMilestone, bool? isActive, int pageNumber = 1, int pageSize = 10)
    {
        var actualUserId = jwtService.GetUserId().ToInt();

        var query = new GetContractsQuery
        {
            UserId = IsAdmin(httpContextAccessor) ? null : actualUserId,
            Status = status,
            SearchKeyword = searchKeyword,
            PriceFilter = priceFilter,
            IsActive = isActive,
            IsMilestone = isMilestone,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await sender.Send(query);

        return TypedResults.Ok(Result<PaginatedList<ContractDetailsDTO>>.Success(
            StatusCodes.Status200OK, "Contracts retrieved successfully.", result));
    }

    [Authorize]
    public async Task<IResult> ModifyContract(ISender sender, IJwtService jwtService, IHttpContextAccessor httpContextAccessor, ModifyContractCommand command)
    {
        var result = await sender.Send(command);

        if (result.Status != StatusCodes.Status200OK)
            return TypedResults.BadRequest(Result<object>.Failure(result.Status, result.Message));

        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Contract modified successfully.", new()));
    }
}
