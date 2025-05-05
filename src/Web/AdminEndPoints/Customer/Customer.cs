using Escrow.Api.Application.Customer.Commands;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Escrow.Api.Application.Customers.Commands;
using Escrow.Api.Application.Customers.Queries;
using Escrow.Api.Application.Customer.Command;
using Escrow.Api.Application.Customer.Queries;

namespace Escrow.Api.Web.AdminEndPoints.Customers;

public class Customers : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var customerGroup = app.MapGroup(this)
        .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin))) // Enable OpenIddict authorization
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) =>
        {
            return await next(context);
        });

        customerGroup.MapPost("/add", AddCustomer);
        customerGroup.MapPut("/edit", EditCustomer);
        customerGroup.MapPut("/delete/{id:int}", DeleteCustomer);
        customerGroup.MapPut("/update-status/{id:int}", ChangeCustomerStatus); // New endpoint for toggling status
        customerGroup.MapGet("/list", GetCustomers);
        customerGroup.MapGet("/detail/{id:int}", GetCustomers);

        customerGroup.MapGet("/contract-details/{id:int}", CustomerContractDetails);


    }

    [Authorize]
    public async Task<IResult> AddCustomer(ISender sender, CreateCustomerCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Customer created successfully.", new { CustomerId = id }));
    }

    [Authorize]
    public async Task<IResult> EditCustomer(ISender sender, UpdateCustomerCommand command)
    {
        var result = await sender.Send(command);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Customer updated successfully.", new()));
    }

    [Authorize]
    public async Task<IResult> DeleteCustomer(ISender sender, int id, int deletedBy)
    {
        var command = new DeleteCustomerCommand(id, deletedBy);
        var result = await sender.Send(command);

        if (result.Status == StatusCodes.Status404NotFound)
        {
            return TypedResults.NotFound(Result<object>.Failure(result.Status, result.Message ?? "Customer not found."));
        }

        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Customer deleted successfully."));
    }

    [Authorize]
    public async Task<IResult> GetCustomers(ISender sender, [AsParameters] GetCustomerQuery request)
    {
        var result = await sender.Send(request);

        if (result == null || result.Items.Count == 0)
        {
            return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "No customers found."));
        }

        return TypedResults.Ok(Result<PaginatedList<CustomerDto>>.Success(StatusCodes.Status200OK, "Customers retrieved successfully.", result));
    }

    [Authorize]
    public async Task<IResult> ChangeCustomerStatus(ISender sender, int id, int updatedBy)
    {
        var command = new ChangeCustomerStatusCommand { Id = id, UpdatedBy = updatedBy };
        var result = await sender.Send(command);

        if (result.Status == StatusCodes.Status404NotFound)
        {
            return TypedResults.NotFound(Result<object>.Failure(result.Status, result.Message ?? "Customer not found."));
        }

        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Customer status updated successfully."));
    }


    [Authorize]
    public async Task<IResult> CustomerContractDetails(ISender sender,int activePageNumber = 1,int historicalPageNumber = 1,int activePageSize = 10,int historicalPageSize = 10,string? id = null)
    {
        // Create the query with all parameters
        var query = new GetCustomerContractsQuery
        {
            ActivePageNumber = activePageNumber,
            HistoricalPageNumber = historicalPageNumber,
            ActivePageSize = activePageSize,
            HistoricalPageSize = historicalPageSize,
            CustomerId = id
        };

        // Send the query to the handler
        var result = await sender.Send(query);

        // Check the result status and handle accordingly
        if (result.Status == StatusCodes.Status404NotFound)
        {
            return TypedResults.NotFound(Result<object>.Failure(result.Status, result.Message ?? "No contracts found for the given customer."));
        }

        return TypedResults.Ok(result);
    }

    //[Authorize]
    //public async Task<IResult> CustomerContractDetails(ISender sender, string? id)
    //{
    //    var query = new GetCustomerContractsQuery { CustomerId = id };
    //    var result = await sender.Send(query);

    //    if (result.Status == StatusCodes.Status404NotFound)
    //    {
    //        return TypedResults.NotFound(Result<object>.Failure(result.Status, result.Message ?? "Customer not found."));
    //    }

    //    return TypedResults.Ok(result);
    //}

}
