using System.Net;
using Microsoft.AspNetCore.Authorization;
using Escrow.Api.Application.UserPanel.Commands.UpdateUser;
using Microsoft.AspNetCore.Http.HttpResults;
using Twilio.TwiML.Messaging;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.ContractPanel.ContractCommands;
using Escrow.Api.Application.ContractPanel.ContractQueries;
using Escrow.Api.Application;
using Escrow.Api.Application.Common.Models.ContractDTOs;

using Escrow.Api.Application.ContractPanel;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.ContractPanel.MilestoneQueries;


namespace Escrow.Api.Web.Endpoints.ContractPanel;

public class Contract : EndpointGroupBase
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

        userGroup.MapGet("/", GetContractDetails).RequireAuthorization(policy => policy.RequireRole("User"));
        userGroup.MapGet("/allcontracts", GetAllContractDetails).RequireAuthorization(policy => policy.RequireRole("User"));
        userGroup.MapPost("/", CreateContractDetails).RequireAuthorization(p => p.RequireRole("User"));
        userGroup.MapPost("/update", UpdateContractDetail).RequireAuthorization(policy => policy.RequireRole("User"));
    }

    [Authorize]
    public async Task<IResult> GetContractDetails(ISender sender, IJwtService jwtService, int? contractId)
    {
        var query = new GetContractForUserQuery { Id = jwtService.GetUserId().ToInt(), ContractId = contractId, PageNumber = 1, PageSize = 10 };
        var result = await sender.Send(query);
        return TypedResults.Ok(Result<PaginatedList<ContractDetailsDTO>>.Success(StatusCodes.Status200OK, "Success", result));
    }

    [Authorize]
    public async Task<IResult> GetAllContractDetails(ISender sender, IJwtService jwtService)
    {
        var query = new GetContractForUserQuery { Id = jwtService.GetUserId().ToInt(), PageNumber = 1, PageSize = 10 };
        var result = await sender.Send(query);
        return TypedResults.Ok(Result<PaginatedList<ContractDetailsDTO>>.Success(StatusCodes.Status200OK, "Success", result));
    }

    [Authorize]
    public async Task<IResult> CreateContractDetails(ISender sender, CreateContractDetailCommand command)
    {
        var id = await sender.Send(command);        
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent, "Contract Created Successfully.",new()));
    }

    [Authorize]
    public async Task<IResult> UpdateContractDetail(ISender sender, EditContractDetailCommand command)
    {
        await sender.Send(command);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent, "Contract details updated successfully.", new()));
    }
}
