using Escrow.Api.Application.AdminAuth.Commands;
using Escrow.Api.Application.AdminAuth.Queries;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Escrow.Api.Web.AdminEndPoints.AdminAuth;

public class AdminAuth : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
            .WithOpenApi();

        userGroup.MapPost("/Login", AdminLogin); // public
        userGroup.MapPost("/ForgotPassword", AdminForgotPassword); // public
        userGroup.MapPost("/VerifyOTP", VerifyOTP); // public
        userGroup.MapPost("/ResetPassword", ResetPassword); // public

        userGroup.MapPost("/ChangePassword", AdminChangePassword)
                  .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.SubAdmin), nameof(Roles.Admin)));

        userGroup.MapPost("/GetAdminAllDetail", GetAdminAllDetail)
                 .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.SubAdmin), nameof(Roles.Admin)));

        userGroup.MapPut("/UpdateDetails", UpdateDetails)
                 .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.SubAdmin), nameof(Roles.Admin)));

        userGroup.MapGet("/GetAdminListings", GetAdminListings)
                 .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.SubAdmin), nameof(Roles.Admin)));
    }

    //public override void Map(WebApplication app)
    //{
    //    var userGroup = app.MapGroup(this)
    //        .WithOpenApi()
    //        .AddEndpointFilter(async (context, next) =>
    //        {
    //            return await next(context);
    //        });

    //    userGroup.MapPost("/Login", AdminLogin);
    //    userGroup.MapPost("/ForgotPassword", AdminForgotPassword);
    //    userGroup.MapPost("/VerifyOTP", VerifyOTP);
    //    userGroup.MapPost("/ResetPassword", ResetPassword);
    //    userGroup.MapPost("/ChangePassword", AdminChangePassword);
    //    userGroup.MapPost("/GetAdminAllDetail", GetAdminAllDetail);
    //    userGroup.MapPut("/UpdateDetails", UpdateDetails);
    //    userGroup.MapGet("/GetAdminListings", GetAdminListings); 
    //}

    public async Task<IResult> AdminLogin(ISender sender, [AsParameters] GetAdminDetailQuery request)
    {
        var result = await sender.Send(request);

        if (result.Status == 1)
        {
            return TypedResults.BadRequest(result);
        }

        return TypedResults.Ok(result);
    }

    public async Task<IResult> AdminForgotPassword(ISender sender, AdminForgotPasswordCommand command)
    {
        var result = await sender.Send(command);

        if (result.Status == 1)
        {
            return TypedResults.BadRequest(result);
        }

        return TypedResults.Ok(result);
    }

    public async Task<IResult> VerifyOTP(ISender sender, AdminVerifyOTPCommand command)
    {
        var result = await sender.Send(command);

        if (result.Status == 1)
        {
            return TypedResults.BadRequest(result);
        }

        return TypedResults.Ok(result);
    }

    public async Task<IResult> ResetPassword(ISender sender, AdminResetPasswordCommand command)
    {
        var result = await sender.Send(command);

        if (result.Status == 1)
        {
            return TypedResults.BadRequest(result);
        }

        return TypedResults.Ok(result);
    }

    public async Task<IResult> AdminChangePassword(ISender sender, AdminChangePasswordCommand command)
    {
        var result = await sender.Send(command);
        return result.Status == 1 ? TypedResults.BadRequest(result) : TypedResults.Ok(result);
    }

    // New endpoint for GetAdminAllDetailQuery
    public async Task<IResult> GetAdminAllDetail(ISender sender, [AsParameters] GetAdminAllDetailQuery request)
    {
        var result = await sender.Send(request);

        // Return BadRequest if status is 1, otherwise return OK with the result
        return result.Status == 1 ? TypedResults.BadRequest(result) : TypedResults.Ok(result);
    }

    public async Task<IResult> UpdateDetails(ISender sender, UpdateDetailsCommand command)
    {
        var result = await sender.Send(command);
        return result.Status == 1 ? TypedResults.BadRequest(new { Success = false, Message = result.Message }) : TypedResults.Ok(new { Success = true, Message = result.Message });
    }

    public async Task<IResult> GetAdminListings(ISender sender, [AsParameters] GetAdminListingsQuery request)
    {
        var result = await sender.Send(request);
        return result.Status == 1 ? TypedResults.BadRequest(result) : TypedResults.Ok(result);
    }
}
