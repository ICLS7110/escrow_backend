using Escrow.Api.Application.BankDetails.Commands;
using Escrow.Api.Application.BankDetails.Queries;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.ContactUsCommands.Commands;
using Escrow.Api.Application.ContactUsCommands.Queries;
using Escrow.Api.Application.ResultHandler;
using Escrow.Api.Domain.Entities.ContactUs;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using FluentValidation;
namespace Escrow.Api.Web.Endpoints.ContactUss;

public class ContactUs : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization(p => p.RequireRole("User")) // Enable OpenIddict authorization
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) =>
        {
            // Optional: Add custom authorization logic if needed
            return await next(context);
        });                 
        userGroup.MapPost("/", CreateContactUsDetail);
        
    }

    //[Authorize]
    [AllowAnonymous]
    public async Task<IResult> CreateContactUsDetail(ISender sender, CreateContactUsCommand command)
    {
        
        var id = await sender.Send(command);
        return TypedResults.Ok(Result<object>.Success(new { Message = "Query Successfully Submitted." }));
    }
}
