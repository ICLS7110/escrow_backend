using Escrow.Api.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Escrow.Api.Application.Admin.Commands;
using Escrow.Api.Application.AdminAuth.Commands;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Domain.Enums;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Web.AdminEndPoints.SubAdmin;

public class SubAdmin : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var subAdminGroup = app.MapGroup(this)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Roles.Admin))) // Enable OpenIddict authorization
            .WithOpenApi()
            .AddEndpointFilter(async (context, next) =>
            {
                return await next(context);
            });

        subAdminGroup.MapPost("/Add", AddSubAdmin);
        subAdminGroup.MapDelete("/Delete/{id:int}", DeleteSubAdmin);
        subAdminGroup.MapPatch("/status/{id:int}", ChangeStatus);
    }

    public async Task<IResult> AddSubAdmin(ISender sender, CreateSubAdminCommand command)
    {
        try
        {
            var result = await sender.Send(command);
            return TypedResults.Ok(new { Success = true, SubAdminId = result });
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    public async Task<IResult> DeleteSubAdmin(ISender sender, int id, int deletedBy, IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        try
        {
            var command = new DeleteCommand { Id = id, DeletedBy = deletedBy };
            var result = await sender.Send(command);

            if (result.Status == StatusCodes.Status404NotFound)
            {
                var message = AppMessages.Get(result.Message ?? "DeleteFailed", language);
                return TypedResults.BadRequest(new { Success = false, Message = message });
            }

            var successMessage = AppMessages.Get("DeleteSuccess", language);
            return TypedResults.Ok(new { Success = true, Message = successMessage });
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    //public async Task<IResult> DeleteSubAdmin(ISender sender, int id, int deletedBy)
    //{
    //    try
    //    {
    //        var command = new DeleteCommand { Id = id, DeletedBy = deletedBy };
    //        var result = await sender.Send(command);

    //        if (result.Status == 1)
    //        {
    //            return TypedResults.BadRequest(new { Success = false, Message = result.Message ?? "Failed to delete." });
    //        }

    //        return TypedResults.Ok(new { Success = true, Message = "Deleted successfully." });
    //    }
    //    catch (Exception ex)
    //    {
    //        return TypedResults.BadRequest(new { Success = false, Message = ex.Message });
    //    }
    //}
    //public async Task<IResult> ChangeStatus(ISender sender, int id ,IHttpContextAccessor _httpContextAccessor)
    //{
    //    try
    //    {
    //        var command = new ChangeStatusCommand { Id = id };
    //        var result = await sender.Send(command);

    //        if (result.Status == 1)
    //        {

    //            return TypedResults.BadRequest(new { Success = false, Message = result.Message ?? "Failed to update status." });
    //        }

    //        return TypedResults.Ok(new { Success = true, Message = "Status updated successfully." });
    //    }
    //    catch (Exception ex)
    //    {
    //        return TypedResults.BadRequest(new { Success = false, Message = ex.Message });
    //    }
    //}

    public async Task<IResult> ChangeStatus(ISender sender, int id, IHttpContextAccessor _httpContextAccessor)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        try
        {
            var command = new ChangeStatusCommand { Id = id };
            var result = await sender.Send(command);

            if (result.Status == StatusCodes.Status400BadRequest)
            {
                var failureMessage = AppMessages.Get(result.Message ?? "StatusUpdateFailed", language);
                return TypedResults.BadRequest(new { Success = false, Message = failureMessage });
            }

            var successMessage = AppMessages.Get("StatusUpdateSuccess", language);
            return TypedResults.Ok(new { Success = true, Message = successMessage });
        }
        catch (Exception ex)
        {
            var errorMessage = AppMessages.Get("UnexpectedError", language);
            return TypedResults.BadRequest(new { Success = false, Message = errorMessage, Details = ex.Message });
        }
    }



}
