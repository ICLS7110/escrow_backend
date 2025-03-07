using Escrow.Api.Application.AdminAuth.Commands;
using Escrow.Api.Application.AdminAuth.Queries;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Escrow.Api.Web.AdminEndPoints.AdminAuth;

public class AdminAuth : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
            .WithOpenApi()
            .AddEndpointFilter(async (context, next) =>
            {
                return await next(context);
            });

        userGroup.MapPost("/Login", AdminLogin);
        userGroup.MapPost("/ForgotPassword", AdminForgotPassword);
        userGroup.MapPost("/VerifyOTP", VerifyOTP);
        userGroup.MapPost("/ResetPassword", ResetPassword);
    }

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
}
