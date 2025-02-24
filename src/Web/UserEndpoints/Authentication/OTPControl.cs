namespace Escrow.Api.Web.Endpoints;

using Escrow.Api.Application.Features.Commands;
using Escrow.Api.Application.Features.Queries;


public class OTP : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .AddEndpointFilter(async (context, next) =>
        {
            return await next(context);
        });
        
        userGroup.MapPost("/request-otp", RequestOtp);
        userGroup.MapPost("/verify-otp", VerifyOtp);

    }
    public async Task<IResult> RequestOtp(ISender sender, RequestOTPCommand request)
    {
        var result = await sender.Send(request);
        return TypedResults.Ok(result);
    }
    public async Task<IResult> VerifyOtp(ISender sender, VerifyOTPQuery request)
    {       
        var result = await sender.Send(request);
        return TypedResults.Ok(result);
    }
}
