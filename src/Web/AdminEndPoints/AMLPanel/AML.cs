using Escrow.Api.Application.AML.Queries;
using Escrow.Api.Application.AML.Commands;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Escrow.Api.Application.Common.Models.AML;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Web.AdminEndPoints.AMLPanel;

public class AML : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var amlGroup = app.MapGroup(this)
        .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin))) // Remove if needed
        .WithOpenApi();

        amlGroup.MapGet("/transactions/flagged", GetFlaggedTransactions);
        amlGroup.MapGet("/settings", GetAMLSettings);
        amlGroup.MapPost("/settings", UpdateAMLSettings);
        amlGroup.MapPost("/transactions/verify", VerifyTransaction);
        amlGroup.MapGet("/notifications", GetAMLNotifications);
    }

    //[Authorize]
    //public async Task<IResult> GetFlaggedTransactions(ISender sender, [AsParameters] GetFlaggedTransactionsQuery request)
    //{
    //    var result = await sender.Send(request);

    //    if (result == null || result.Items.Count == 0)
    //    {
    //        // ✅ Return an empty list instead of 404 to avoid breaking the frontend
    //        return TypedResults.Ok(Result<PaginatedList<FlaggedTransactionDto>>.Success(
    //            StatusCodes.Status200OK, "No flagged transactions found.", new PaginatedList<FlaggedTransactionDto>(new List<FlaggedTransactionDto>(), 0, request.PageNumber ?? 1, request.PageSize ?? 10)
    //        ));
    //    }

    //    return TypedResults.Ok(Result<PaginatedList<FlaggedTransactionDto>>.Success(
    //        StatusCodes.Status200OK, "Flagged transactions retrieved successfully.", result));
    //}


    [Authorize]
    public async Task<IResult> GetFlaggedTransactions(ISender sender,[AsParameters] GetFlaggedTransactionsQuery request,IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(request);

        if (result == null || result.Items.Count == 0)
        {
            var emptyMessage = AppMessages.Get("NoFlaggedTransactionsFound", language);

            return TypedResults.Ok(Result<PaginatedList<FlaggedTransactionDto>>.Success(
                StatusCodes.Status200OK,
                emptyMessage,
                new PaginatedList<FlaggedTransactionDto>(
                    new List<FlaggedTransactionDto>(),
                    0,
                    request.PageNumber ?? 1,
                    request.PageSize ?? 10
                )
            ));
        }

        var successMessage = AppMessages.Get("FlaggedTransactionsRetrieved", language);

        return TypedResults.Ok(Result<PaginatedList<FlaggedTransactionDto>>.Success(
            StatusCodes.Status200OK,
            successMessage,
            result
        ));
    }


    [Authorize]
    public async Task<IResult> GetAMLSettings(ISender sender, IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(new GetAMLSettingsQuery());

        if (result.Data == null)
        {
            var message = AppMessages.Get("NoAMLSettingsFound", language);
            return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, message, new { }));
        }

        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> UpdateAMLSettings(ISender sender, UpdateAMLSettingsCommand command, IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(command);
        var message = AppMessages.Get("AMLSettingsUpdated", language);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, message, result.Data));
    }

    [Authorize]
    public async Task<IResult> VerifyTransaction(ISender sender, AMLTransactionVerificationCommand command, IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(command);
        var message = AppMessages.Get("TransactionVerificationUpdated", language);
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, message));
    }

    [Authorize]
    public async Task<IResult> GetAMLNotifications(ISender sender, IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var result = await sender.Send(new GetAMLNotificationsQuery());

        if (result.Data == null || !result.Data.Any())
        {
            var emptyMessage = AppMessages.Get("NoAMLNotificationsFound", language);
            return TypedResults.Ok(Result<List<AMLNotificationDto>>.Success(StatusCodes.Status200OK, emptyMessage, new List<AMLNotificationDto>()));
        }

        var successMessage = AppMessages.Get("AMLNotificationsRetrieved", language);
        return TypedResults.Ok(Result<List<AMLNotificationDto>>.Success(StatusCodes.Status200OK, successMessage, result.Data));
    }






    //[Authorize]
    //public async Task<IResult> GetAMLSettings(ISender sender)
    //{
    //    var result = await sender.Send(new GetAMLSettingsQuery());

    //    if (result.Data == null)
    //    {
    //        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "No AML settings found.", new { }));
    //    }

    //    return TypedResults.Ok(result);
    //}


    //[Authorize]
    //public async Task<IResult> UpdateAMLSettings(ISender sender, UpdateAMLSettingsCommand command)
    //{
    //    var result = await sender.Send(command);
    //    return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "AML settings updated successfully.", result.Data));
    //}

    //[Authorize]
    //public async Task<IResult> VerifyTransaction(ISender sender, AMLTransactionVerificationCommand command)
    //{
    //    var result = await sender.Send(command);
    //    return TypedResults.Ok(Result<object>.Success(StatusCodes.Status200OK, "Transaction verification updated successfully."));
    //}

    //[Authorize]
    //public async Task<IResult> GetAMLNotifications(ISender sender)
    //{
    //    var result = await sender.Send(new GetAMLNotificationsQuery());

    //    if (result.Data == null || !result.Data.Any())
    //    {
    //        // ✅ Return empty list instead of 404
    //        return TypedResults.Ok(Result<List<AMLNotificationDto>>.Success(StatusCodes.Status200OK, "No AML notifications found.", new List<AMLNotificationDto>()));
    //    }

    //    return TypedResults.Ok(Result<List<AMLNotificationDto>>.Success(StatusCodes.Status200OK, "AML notifications retrieved successfully.", result.Data));
    //}
}
