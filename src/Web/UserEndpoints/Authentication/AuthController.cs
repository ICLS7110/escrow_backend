using Escrow.Api.Application.Common.Models.Dto;
using Escrow.Api.Infrastructure.Authentication.Services;
using Escrow.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using System.Security.Claims;

using Escrow.Api.Application.Authentication.Interfaces;
using Microsoft.IdentityModel.Tokens;
using static OpenIddict.Abstractions.OpenIddictConstants;
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

namespace Escrow.Api.Web.Endpoints.Authentication
{


    public class Auth : EndpointGroupBase
    {
        public override void Map(WebApplication app)
        {
            var userGroup = app.MapGroup(this)
            .WithOpenApi()
            .AddEndpointFilter(async (context, next) =>
            {
                // Optional: Add custom authorization logic if needed
                return await next(context);
            });

            userGroup.MapPost("/request-otp", RequestOtp);
            userGroup.MapPost("/verify-otp", VerifyOtp);
        }
        [AllowAnonymous]
        public async Task<IResult> RequestOtp([FromServices] ISender sender, [FromBody] RequestOtpQuery query)
        {
            var result = await sender.Send(query);
            return TypedResults.Ok(result);
        }
        [AllowAnonymous]
        public async Task<IResult> VerifyOtp([FromServices] ISender sender, [FromBody] VerifyOTPQuery command) 
        {

            var result = await sender.Send(command);
            return TypedResults.Ok(result);

        }

    }
}
