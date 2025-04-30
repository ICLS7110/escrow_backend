using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using Escrow.Api.Application.AdminDashboard.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web.AdminEndPoints.AdminDashboard;

public class Dashboard : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var dashboardGroup = app.MapGroup(this)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin)))
            .WithOpenApi();

        dashboardGroup.MapGet("/users-count", GetUserCount);
        dashboardGroup.MapGet("/contracts-worth", GetContractsWorth);
        dashboardGroup.MapGet("/admin-commission-last-12-months", GetAdminCommissionLast12Months);
        //dashboardGroup.MapGet("/projected-commission-next-6-months", GetProjectedCommissionNext6Months);
        dashboardGroup.MapGet("/escrow-amount", GetAmountInEscrow);


        //new Api's For DAshBoard Counts
        dashboardGroup.MapGet("/dashboard-counts", GetDashboardCounts);
        dashboardGroup.MapGet("/dashboard-listings", GetDashboardListings);
    }

    [Authorize]
    public async Task<IResult> GetUserCount(ISender sender)
    {
        var result = await sender.Send(new GetUserCountQuery());
        return TypedResults.Ok(result); // already a Result<int>
    }

    [Authorize]
    public async Task<IResult> GetContractsWorth(ISender sender, [AsParameters] GetContractsWorthQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result); 
    }


    [Authorize]
    public async Task<IResult> GetAdminCommissionLast12Months(ISender sender, [FromQuery] int? year, [FromQuery] int? month)
    {
        var query = new GetAdminCommissionLast12MonthsQuery
        {
            Year = year,
            Month = month
        };

        var result = await sender.Send(query);
        return TypedResults.Ok(result); // Already formatted by handler
    }

    

    [Authorize]
    public async Task<IResult> GetAmountInEscrow(ISender sender)
    {
        var result = await sender.Send(new GetEscrowAmountQuery());
        return TypedResults.Ok(result);
    }








    [Authorize]
    public async Task<IResult> GetDashboardCounts(ISender sender)
    {
        var result = await sender.Send(new GetDashboardCountsQuery());
        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> GetDashboardListings(ISender sender)
    {
        var result = await sender.Send(new GetDashboardListingsQuery());
        return TypedResults.Ok(result);
    }



}
