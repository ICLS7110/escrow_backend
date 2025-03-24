using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.ContractPanel.ContractCommands;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web.UserEndpoints.ContractPanel;

public class SellerBuyer : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization() // Enforce authentication
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) =>
        {
            return await next(context);
        });

        userGroup.MapPost("/", CreateSellerBuyerInvitation).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));
    }

    [Authorize]
    public async Task<IResult> CreateSellerBuyerInvitation(ISender sender, CreateBuyerSellerCommand command)
    {
        // Send the command to process seller-buyer creation
        var invitationLink = await sender.Send(command);

        // Return response with generated ID
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Invitation Created Successfully.", new { Id = invitationLink }));
    }
}
