using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Transactions.Commands;
using Escrow.Api.Application.Transactions.Queries;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Escrow.Api.Web.UserEndpoints.Transactions;

public class Transactions : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var adminGroup = app.MapGroup(this)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User))) // Restrict access to Admins & Users
            .WithOpenApi();

        adminGroup.MapGet("/{transaction_id}", GetTransactionById);
        adminGroup.MapGet("/search", SearchTransactions);
        adminGroup.MapPost("/export", ExportTransactions);
        adminGroup.MapPost("/create", CreateTransaction); // ✅ New endpoint for creating transactions
    }

    ///// <summary>
    ///// Retrieves transaction details by ID.
    ///// </summary>
    //[Authorize]
    //public async Task<IResult> GetTransactionById(ISender sender, int transaction_id)
    //{
    //    var result = await sender.Send(new GetTransactionByIdQuery { TransactionId = transaction_id });

    //    if (result == null)
    //    {
    //        return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "Transaction not found."));
    //    }

    //    return TypedResults.Ok(result);
    //}

    ///// <summary>
    ///// Searches and filters transactions based on query parameters with pagination.
    ///// </summary>
    //[Authorize]
    //public async Task<IResult> SearchTransactions(ISender sender, [AsParameters] SearchTransactionsRequestDTO request)
    //{
    //    var result = await sender.Send(new SearchTransactionsQuery
    //    {
    //        Keyword = request.Keyword,
    //        TransactionStatus = request.TransactionStatus,
    //        TransactionType = request.TransactionType,
    //        StartDate = request.StartDate,
    //        EndDate = request.EndDate,
    //        PageNumber = request.PageNumber,
    //        PageSize = request.PageSize
    //    });

    //    if (result == null)
    //    {
    //        return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "No transactions found."));
    //    }

    //    return TypedResults.Ok(result);
    //}



    ///// <summary>
    ///// Creates a new transaction.
    ///// </summary>
    //[Authorize]
    //public async Task<IResult> CreateTransaction(ISender sender, CreateTransactionCommand command)
    //{
    //    // Validate the request payload
    //    if (command == null)
    //    {
    //        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid request payload."));
    //    }

    //    try
    //    {
    //        // Execute the command to create the transaction
    //        var result = await sender.Send(command);

    //        // Check if the operation was unsuccessful based on the result status
    //        if (result.Status != StatusCodes.Status200OK) // Check if the operation was unsuccessful
    //        {
    //            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, result.Message ?? "Team creation failed."));
    //        }

    //        return TypedResults.Ok(result);
    //    }
    //    catch (Exception ex)
    //    {
    //        // Log the error (replace with actual logging if available)
    //        Console.WriteLine($"Error creating transaction: {ex.Message}");

    //        // Return an internal server error response if an exception occurs
    //        return TypedResults.Json(
    //            Result<object>.Failure(StatusCodes.Status500InternalServerError, "An unexpected server error occurred."),
    //            statusCode: StatusCodes.Status500InternalServerError
    //        );
    //    }
    //}



    [Authorize]
    public async Task<IResult> GetTransactionById(ISender sender, IHttpContextAccessor httpContextAccessor, int transaction_id)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(new GetTransactionByIdQuery { TransactionId = transaction_id });

        if (result == null)
        {
            return TypedResults.NotFound(Result<object>.Failure(
                StatusCodes.Status404NotFound,
                AppMessages.Get("TransactionNotFound", language)
            ));
        }

        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> SearchTransactions(ISender sender, IHttpContextAccessor httpContextAccessor, [AsParameters] SearchTransactionsRequestDTO request)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(new SearchTransactionsQuery
        {
            Keyword = request.Keyword,
            TransactionStatus = request.TransactionStatus,
            TransactionType = request.TransactionType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });

        if (result == null)
        {
            return TypedResults.NotFound(Result<object>.Failure(
                StatusCodes.Status404NotFound,
                AppMessages.Get("NoTransactionsFound", language)
            ));
        }

        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> CreateTransaction(ISender sender, IHttpContextAccessor httpContextAccessor, CreateTransactionCommand command)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        if (command == null)
        {
            return TypedResults.BadRequest(Result<object>.Failure(
                StatusCodes.Status400BadRequest,
                AppMessages.Get("InvalidRequestPayload", language)
            ));
        }

        try
        {
            var result = await sender.Send(command);

            if (result.Status != StatusCodes.Status200OK)
            {
                return TypedResults.BadRequest(Result<object>.Failure(
                    StatusCodes.Status400BadRequest,
                    result.Message ?? AppMessages.Get("TransactionCreationFailed", language)
                ));
            }

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating transaction: {ex.Message}");

            return TypedResults.Json(
                Result<object>.Failure(StatusCodes.Status500InternalServerError, AppMessages.Get("UnexpectedServerError", language)),
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }



    /// <summary>
    /// Exports transaction data based on filter criteria.
    /// </summary>
    [Authorize]
    public async Task<IResult> ExportTransactions(ISender sender, ExportTransactionsCommand command)
    {
        var result = await sender.Send(command);

        return result.Status switch
        {
            StatusCodes.Status400BadRequest => TypedResults.BadRequest(result),
            StatusCodes.Status404NotFound => TypedResults.NotFound(result),
            _ => TypedResults.Ok(result)
        };
    }


}
