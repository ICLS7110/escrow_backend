using Escrow.Api.Application;
using Escrow.Api.Application.BankDetails.Queries;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Helpers;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.UserPanel.Commands;
using Escrow.Api.Application.UserPanel.Commands.CreateUser;
using Escrow.Api.Application.UserPanel.Commands.DeleteUser;
using Escrow.Api.Application.UserPanel.Commands.UpdateUser;
using Escrow.Api.Application.UserPanel.Queries.GetUsers;
using Escrow.Api.Domain.Enums;
using Escrow.Api.Infrastructure.Configuration;
using Escrow.Api.Web.UserEndpoints.BankDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Escrow.Api.Web.Endpoints.UserPanel;

public class UserDetails : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization()
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) =>
        {
            return await next(context);
        });

        // User management
        userGroup.MapGet("/", GetUserDetails).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));
        userGroup.MapPost("/update", UpdateUserDetail).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));
        userGroup.MapPut("/delete-user", DeleteUser).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));

        // Device token management
        userGroup.MapPost("/device-token", StoreDeviceToken).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User),nameof(Roles.Admin),nameof(Roles.SubAdmin)));
        userGroup.MapPost("/set-notified-status", UpdateNotificationStatus).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));

        // Social logins
        userGroup.MapPost("/auth/SocialMeadiaLogin", SocialMediaLogin).AllowAnonymous();

        userGroup.MapPost("/update-mobile-number", UpdateMobileNumber).AllowAnonymous();
        userGroup.MapPost("/send-sms", SendSMS).AllowAnonymous();

        // Virtual IBAN and Customer Reference
        userGroup.MapPost("/generate-iban", GenerateVirtualIban).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));
        userGroup.MapPost("/generate-customer-reference", GenerateCustomerReference).RequireAuthorization(policy => policy.RequireRole(nameof(Roles.User)));


    }

    //[Authorize]
    //public async Task<IResult> GetUserDetails(ISender sender, IJwtService jwtService)
    //{
    //    try
    //    {
    //        // 🔹 Ensure we get a valid user ID from the token
    //        int? userId = jwtService.GetUserId()?.ToInt();

    //        if (userId == null || userId <= 0)
    //        {
    //            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid user ID"));
    //        }

    //        var query = new GetUserDetailsQuery { Id = userId, PageNumber = 1, PageSize = 1 };
    //        var result = await sender.Send(query);

    //        if (result == null)
    //        {
    //            return TypedResults.NotFound(Result<object>.Failure(StatusCodes.Status404NotFound, "User not found"));
    //        }

    //        return TypedResults.Ok(Result<PaginatedList<UserDetailDto>>.Success(StatusCodes.Status200OK, "Success", result));
    //    }
    //    catch (Exception ex)
    //    {
    //        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, ex.Message));
    //    }
    //}

    //[Authorize]
    //public async Task<IResult> UpdateUserDetail(ISender sender, UpdateUserCommand command)
    //{
    //    try
    //    {
    //        await sender.Send(command);
    //        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent, "User details updated successfully.", new()));
    //    }
    //    catch (Exception ex)
    //    {
    //        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, ex.Message));
    //    }
    //}

    //[Authorize]
    //public async Task<IResult> DeleteUser(ISender sender, IJwtService jwtService)
    //{
    //    try
    //    {
    //        var userId = jwtService.GetUserId().ToInt();
    //        await sender.Send(new DeleteUserCommand(userId));
    //        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent, "User details deleted successfully.", new()));
    //    }
    //    catch (Exception ex)
    //    {
    //        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, ex.Message));
    //    }
    //}

    //[AllowAnonymous]
    //public async Task<IResult> SocialMeadiaLogin(ISender sender, SocialLoginCommand command)
    //{
    //    return await HandleSocialLogin(sender, command, command.Provider, "Google login successful");
    //}
    //private async Task<IResult> HandleSocialLogin(ISender sender, SocialLoginCommand command, string provider, string successMessage)
    //{
    //    try
    //    {

    //        command.Provider = provider;
    //        var result = await sender.Send(command);

    //        if (result.Data == null)
    //        {
    //            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid login attempt."));
    //        }

    //        return TypedResults.Ok(Result<UserLoginDto>.Success(StatusCodes.Status200OK, successMessage, result.Data));
    //    }
    //    catch (Exception ex)
    //    {
    //        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, $"Login failed: {ex.Message}"));
    //    }
    //}


    [Authorize]
    public async Task<IResult> GetUserDetails(ISender sender, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        try
        {
            int? userId = jwtService.GetUserId()?.ToInt();

            if (userId == null || userId <= 0)
            {
                return TypedResults.BadRequest(Result<object>.Failure(
                    StatusCodes.Status400BadRequest,
                    AppMessages.Get("InvalidUserId", language)
                ));
            }

            var query = new GetUserDetailsQuery { Id = userId.Value, PageNumber = 1, PageSize = 1 };
            var result = await sender.Send(query);

            if (result == null)
            {
                return TypedResults.NotFound(Result<object>.Failure(
                    StatusCodes.Status404NotFound,
                    AppMessages.Get("UserNotFound", language)
                ));
            }

            return TypedResults.Ok(Result<PaginatedList<UserDetailDto>>.Success(
                StatusCodes.Status200OK,
                AppMessages.Get("Success", language),
                result
            ));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(Result<object>.Failure(
                StatusCodes.Status400BadRequest,
                ex.Message
            ));
        }
    }

    [Authorize]
    public async Task<IResult> UpdateUserDetail(ISender sender, UpdateUserCommand command, IHttpContextAccessor httpContextAccessor)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        try
        {
            await sender.Send(command);

            return TypedResults.Ok(Result<object>.Success(
                StatusCodes.Status204NoContent,
                AppMessages.Get("UserDetailsUpdated", language),
                new()
            ));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(Result<object>.Failure(
                StatusCodes.Status400BadRequest,
                ex.Message
            ));
        }
    }

    [Authorize]
    public async Task<IResult> DeleteUser(ISender sender, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
    {
        var language = httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        try
        {
            var userId = jwtService.GetUserId()?.ToInt();

            if (userId == null || userId <= 0)
            {
                return TypedResults.BadRequest(Result<object>.Failure(
                    StatusCodes.Status400BadRequest,
                    AppMessages.Get("InvalidUserId", language)
                ));
            }

            await sender.Send(new DeleteUserCommand(userId.Value));

            return TypedResults.Ok(Result<object>.Success(
                StatusCodes.Status204NoContent,
                AppMessages.Get("UserDeleted", language),
                new()
            ));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(Result<object>.Failure(
                StatusCodes.Status400BadRequest,
                ex.Message
            ));
        }
    }

    [AllowAnonymous]
    public async Task<IResult> SocialMediaLogin(ISender sender, SocialLoginCommand command)
    {
        return await HandleSocialLogin(sender, command, command.Provider, "SocialLoginSuccess");
    }

    private async Task<IResult> HandleSocialLogin(ISender sender, SocialLoginCommand command, string provider, string successMessageKey)
    {
        try
        {
            command.Provider = provider;
            var result = await sender.Send(command);

            if (result?.Data == null)
            {
                return TypedResults.BadRequest(Result<object>.Failure(
                    StatusCodes.Status400BadRequest,
                    "Invalid login attempt."
                ));
            }

            return TypedResults.Ok(Result<UserLoginDto>.Success(
                StatusCodes.Status200OK,
                successMessageKey,
                result.Data
            ));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(Result<object>.Failure(
                StatusCodes.Status400BadRequest,
                $"Login failed: {ex.Message}"
            ));
        }
    }

    [Authorize]
    public async Task<IResult> StoreDeviceToken(ISender sender, StoreDeviceTokenCommand command)
    {
        try
        {
            // Send the command to the handler
            var result = await sender.Send(command);
            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            // Return failure response in case of an exception
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [Authorize]
    public async Task<IResult> UpdateNotificationStatus(ISender sender)
    {
        try
        {
            var query = new UpdateNotificationStatusCommand(); 
            var result = await sender.Send(query);
            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [AllowAnonymous]
    public async Task<IResult> UpdateMobileNumber([FromServices] ISender sender, [FromBody] UpdateMobileNumberCommand  query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }


    [AllowAnonymous]
    public async Task<IResult> SendSMS(HttpContext httpContext, [FromBody] SMSRequest request)
    {
        try
        {
            var smsService = httpContext.RequestServices.GetRequiredService<UnifonicSmsService>();
            var response = await smsService.SendSmsAsync(request.To, request.To.ToString(), request.SenderId.ToString());
            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, ex.Message));
        }
    }


    [Authorize]
    public async Task<IResult> GenerateVirtualIban([FromBody] VirtualIbanRequest request)
    {
        try
        {
            var iban = await Task.Run(() =>
            {
                var ibanService = new VirtualIbanService(request.CompanyId, request.BankCode);
                return ibanService.GenerateVirtualIban(request.CustomerId);
            });

            return TypedResults.Ok(Result<string>.Success(StatusCodes.Status200OK, "Virtual IBAN generated successfully.", iban));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, ex.Message));
        }
    }

    [Authorize]
    public async Task<IResult> GenerateCustomerReference([FromBody] ReferenceRequest request)
    {
        try
        {
            var reference = await Task.Run(() =>
            {
                var paymentRefService = new PaymentReferenceService();
                return paymentRefService.GenerateReference(
                    request.PartyIndicator,
                    request.CustomerId,
                    request.ContractId,
                    request.MilestoneId
                );
            });

            if (string.IsNullOrEmpty(reference))
                return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Failed to generate payment reference."));

            return TypedResults.Ok(Result<string>.Success(StatusCodes.Status200OK, "Payment reference generated successfully.", reference));
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, ex.Message));
        }
    }










}











//[Authorize]
//public async Task<IResult> GenerateVirtualIban([FromBody] VirtualIbanRequest request)
//{
//    try
//    {
//        var iban = await Task.Run(() =>
//        {
//            var ibanService = new VirtualIbanService(request.CompanyId, request.BankCode);
//            return ibanService.GenerateVirtualIban(request.CustomerId);
//        });

//        return TypedResults.Ok(Result<string>.Success(StatusCodes.Status200OK, "Virtual IBAN generated successfully.", iban));
//    }
//    catch (Exception ex)
//    {
//        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, ex.Message));
//    }
//}


//[Authorize]
//public async Task<IResult> GenerateCustomerReference([FromBody] VirtualIbanRequest request)
//{
//    try
//    {
//        var subAccount = await Task.Run(() =>
//        {
//            var ibanService = new VirtualIbanService(request.CompanyId, request.BankCode);

//            var method = typeof(VirtualIbanService).GetMethod(
//                "GenerateSubAccountNumber",
//                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
//            );

//            return method?.Invoke(ibanService, new object[] { request.CompanyId, request.CustomerId })?.ToString();
//        });

//        if (subAccount == null)
//            return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, "Failed to generate customer reference."));

//        return TypedResults.Ok(Result<string>.Success(StatusCodes.Status200OK, "Customer reference generated successfully.", subAccount));
//    }
//    catch (Exception ex)
//    {
//        return TypedResults.BadRequest(Result<object>.Failure(StatusCodes.Status400BadRequest, ex.Message));
//    }
//}
