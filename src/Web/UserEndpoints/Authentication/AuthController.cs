using Escrow.Api.Application.Common.Models.Dto;
using Escrow.Api.Infrastructure.Authentication.Services;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using Escrow.Api.Application.Authentication.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

using Escrow.Api.Domain.Entities.UserPanel;

using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Common.Models.BankDtos;
using Escrow.Api.Application.Common.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Escrow.Api.Application.BankDetails.Commands;
using Escrow.Api.Application.BankDetails.Queries;
using Microsoft.AspNetCore.Authorization;
using Escrow.Api.Domain.Entities.DTOs;
using Microsoft.AspNetCore.Authentication;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Escrow.Api.Web.Endpoints.Authentication
{

    public class Auth : EndpointGroupBase
    {
        public override void Map(WebApplication app)
        {
            var userGroup = app.MapGroup(this)
    .WithOpenApi()
    .RequireAuthorization(); // THIS is required

            userGroup.MapPost("/request-otp", RequestOtp);
            userGroup.MapPost("/verify-otp", VerifyOtp);
            userGroup.MapGet("/login", Login);
            userGroup.MapGet("/me", GetUserInfo);
        }

        [AllowAnonymous]
        public async Task<IResult> RequestOtp([FromServices] ISender sender, [FromBody] RequestOtpQuery query)
        {
            var result = await sender.Send(query);
            return result.Status == StatusCodes.Status200OK ? TypedResults.Ok(result) : TypedResults.BadRequest(result);
        }

        [AllowAnonymous]
        public async Task<IResult> VerifyOtp([FromServices] ISender sender, [FromBody] VerifyOTPQuery command)
        {
            var result = await sender.Send(command);
            return result.Status == StatusCodes.Status200OK ? TypedResults.Ok(result) : TypedResults.BadRequest(result);
        }

        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IResult Login(HttpContext context)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = "https://welink-sa.com/contact-us"
            };

            return Results.Challenge(props, new[] { "oidc" });
        }



        [Authorize]

        public IResult GetUserInfo(HttpContext context)
        {
            var user = context.User;

            var email = user?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var fullName = user?.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var picture = user?.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            return Results.Ok(new
            {
                Email = email,
                FullName = fullName,
                Picture = picture,
                //AllClaims = user.Claims.Select(c => new { c.Type, c.Value })
            });
        }

    }




    //public class Auth : EndpointGroupBase
    //{
    //    public override void Map(WebApplication app)
    //    {
    //        var userGroup = app.MapGroup(this)
    //        .WithOpenApi()
    //        .AddEndpointFilter(async (context, next) =>
    //        {
    //            // Optional: Add custom authorization logic if needed
    //            return await next(context);
    //        });

    //        userGroup.MapPost("/request-otp", RequestOtp);
    //        userGroup.MapPost("/verify-otp", VerifyOtp);

    //        userGroup.MapGet("/login", Login);
    //        //userGroup.MapGet("/logout", Logout);
    //        userGroup.MapGet("/me", GetUserInfo);
    //    }
    //    [AllowAnonymous]
    //    public async Task<IResult> RequestOtp([FromServices] ISender sender, [FromBody] RequestOtpQuery query)
    //    {
    //        var result = await sender.Send(query);

    //        if (result.Status == StatusCodes.Status200OK)
    //        {
    //            return TypedResults.Ok(result); // or return what makes sense
    //        }

    //        return TypedResults.BadRequest(result); // or Unauthorized/NotFound/etc. depending on failure reason


    //    }

    //    [AllowAnonymous]
    //    public async Task<IResult> VerifyOtp([FromServices] ISender sender, [FromBody] VerifyOTPQuery command)
    //    {
    //        var result = await sender.Send(command);

    //        if (result.Status == StatusCodes.Status200OK)
    //        {
    //            return TypedResults.Ok(result); // or return what makes sense
    //        }

    //        return TypedResults.BadRequest(result); // or Unauthorized/NotFound/etc. depending on failure reason
    //    }

    //    //[AllowAnonymous]
    //    //public async Task<IResult> VerifyOtp([FromServices] ISender sender, [FromBody] VerifyOTPQuery command) 
    //    //{

    //    //    var result = await sender.Send(command);
    //    //    return TypedResults.Ok(result);

    //    //}


    //    [AllowAnonymous]
    //    [ApiExplorerSettings(IgnoreApi = true)]
    //    public IResult Login(HttpContext context)
    //    {
    //        var props = new AuthenticationProperties
    //        {
    //            RedirectUri = "https://welink-sa.com/contact-us" // must match redirect URI registered with Google
    //        };


    //        return Results.Challenge(props, new[] { "oidc" });
    //    }


    //    //[AllowAnonymous]
    //    //public async Task<IResult> Logout(HttpContext context)
    //    //{
    //    //    await context.SignOutAsync("Cookies");
    //    //    await context.SignOutAsync("oidc");

    //    //    return Results.Ok("Logged out.");
    //    //}

    //    [Authorize]
    //    public IResult GetUserInfo(HttpContext context)
    //    {
    //        var claims = context.User.Claims
    //            .Select(c => new { c.Type, c.Value });

    //        return Results.Ok(claims);
    //    }


    //}
}
