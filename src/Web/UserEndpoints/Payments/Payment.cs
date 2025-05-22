using Escrow.Api.Application.Common.Models.Payments;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Payments.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Escrow.Api.Web.UserEndpoints.Payments;

public class Payment : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var paymentGroup = app.MapGroup(this)
           .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.User))) // Only Admins & Users
           .WithOpenApi();
        //var paymentGroup = app.MapGroup(this).AllowAnonymous()
        //    .WithOpenApi();

        paymentGroup.MapPost("/initiate", InitiatePayment);
        paymentGroup.MapPost("/execute", ExecutePayment);
        paymentGroup.MapGet("/callback", PaymentCallback);
        paymentGroup.MapGet("/error", PaymentError);
        paymentGroup.MapPost("/status", GetPaymentStatus);
        paymentGroup.MapPost("/refund", RefundPayment);
        //paymentGroup.MapPost("/direct", DirectPayment);
    }

    [Authorize]
    public async Task<IResult> InitiatePayment(ISender sender, InitiatePaymentCommand command)
    {
        var result = await sender.Send(command);

        if (result == null || result.Data == null || !result.Data.Any())
        {
            return TypedResults.NotFound(Result<List<PaymentMethodDto>>.Failure(StatusCodes.Status404NotFound, "No payment methods found."));
        }

        return TypedResults.Ok(Result<List<PaymentMethodDto>>.Success(StatusCodes.Status200OK, "Payment methods retrieved successfully.", result.Data));
    }

    [Authorize]
    public async Task<IResult> ExecutePayment(ISender sender, ExecutePaymentCommand command)
    {
        var result = await sender.Send(command);

        if (result.Status == StatusCodes.Status400BadRequest)
        {
            return TypedResults.BadRequest(result);
        }

        if (result.Status == StatusCodes.Status404NotFound)
        {
            return TypedResults.NotFound(result);
        }

        return TypedResults.Ok(result);
    }

    [Authorize]
    public Task<IResult> PaymentCallback([FromQuery] string paymentId)
    {
        return Task.FromResult<IResult>(TypedResults.Ok($"Payment successful. Payment ID: {paymentId}"));
    }

    [Authorize]
    public Task<IResult> PaymentError()
    {
        return Task.FromResult<IResult>(TypedResults.BadRequest("Payment failed or was cancelled by the user."));
    }

    [Authorize]
    public async Task<IResult> GetPaymentStatus(ISender sender, GetPaymentStatusCommand command)
    {
        var result = await sender.Send(command);

        if (result.Status == StatusCodes.Status404NotFound)
        {
            return TypedResults.NotFound(result);
        }

        return TypedResults.Ok(result);
    }

    [Authorize]
    public async Task<IResult> RefundPayment(ISender sender, RefundPaymentCommand command)
    {
        var result = await sender.Send(command);

        if (result.Status == StatusCodes.Status400BadRequest)
        {
            return TypedResults.BadRequest(result);
        }

        return TypedResults.Ok(result);
    }

    //public async Task<IResult> DirectPayment(ISender sender, DirectPaymentCommand command)
    //{
    //    var result = await sender.Send(command);

    //    if (result.Status == StatusCodes.Status400BadRequest)
    //    {
    //        return TypedResults.BadRequest(result);
    //    }

    //    return TypedResults.Ok(result);
    //}
}



























//using Escrow.Api.Application.Common.Models.Payments;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Application.Payments.Commands;
//using System;

//namespace Escrow.Api.Web.UserEndpoints.Payments;

//public class Payment : EndpointGroupBase
//{
//    public override void Map(WebApplication app)
//    {
//        var paymentGroup = app.MapGroup(this).AllowAnonymous()
//            .WithOpenApi();

//        paymentGroup.MapPost("/initiate", InitiatePayment);
//        paymentGroup.MapPost("/execute", ExecutePayment);

//        paymentGroup.MapGet("/callback", PaymentCallback);        
//        paymentGroup.MapGet("/error", PaymentError);              // Optional
//        paymentGroup.MapPost("/status", GetPaymentStatus);        // Recommended
//        paymentGroup.MapPost("/refund", RefundPayment);           // Optional
//        paymentGroup.MapPost("/direct", DirectPayment);           // Optional
//    }

//    /// <summary>
//    /// Initiates a payment and returns available payment methods.
//    /// </summary>
//    public async Task<IResult> InitiatePayment(ISender sender, InitiatePaymentCommand command)
//    {
//        var result = await sender.Send(command);

//        if (result == null || result.Data == null || !result.Data.Any())
//        {
//            return TypedResults.NotFound(Result<List<PaymentMethodDto>>.Failure(StatusCodes.Status404NotFound, "No payment methods found."));
//        }

//        return TypedResults.Ok(Result<List<PaymentMethodDto>>.Success(StatusCodes.Status200OK, "Payment methods retrieved successfully.", result.Data));
//    }

//    /// <summary>
//    /// Executes a payment using selected payment method.
//    /// </summary>
//    public async Task<IResult> ExecutePayment(ISender sender, ExecutePaymentCommand command)
//    {
//        var result = await sender.Send(command);

//        if (result.Status == StatusCodes.Status400BadRequest)
//        {
//            return TypedResults.BadRequest(result);
//        }

//        if (result.Status == StatusCodes.Status404NotFound)
//        {
//            return TypedResults.NotFound(result);
//        }

//        return TypedResults.Ok(result);
//    }
//}
