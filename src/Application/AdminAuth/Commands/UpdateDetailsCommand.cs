
using Escrow.Api.Application;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record UpdateDetailsCommand : IRequest<Result<string>>
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
}

public class UpdateDetailsCommandHandler : IRequestHandler<UpdateDetailsCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateDetailsCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<string>> Handle(UpdateDetailsCommand request, CancellationToken cancellationToken)
    {
        // Retrieve the current language from HttpContext
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var adminUser = await _context.UserDetails.FindAsync(new object[] { request.Id }, cancellationToken);

        if (adminUser == null)
        {
            // Return localized failure message for "Admin not found"
            return Result<string>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("AdminNotFound", language));
        }

        // If the provided role matches the existing role, proceed with updating details
        if (adminUser.Role == request.Role)
        {
            adminUser.FullName = request.UserName;
            // If role is NOT "Sub-Admin", update both Name and Email
            if (adminUser.Role.ToLower() == nameof(Roles.Admin).ToLower())
            {
                adminUser.EmailAddress = request.Email;
            }
            adminUser.ProfilePicture = request.Image;
            adminUser.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            // Return localized success message for "Admin updated"
            return Result<string>.Success(StatusCodes.Status200OK, AppMessages.Get("AdminUpdated", language));
        }

        // If roles do not match, return localized error for "Role mismatch"
        return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("RoleMismatch", language));
    }
}



































//using Escrow.Api.Application;
//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Enums;
//using Microsoft.AspNetCore.Http;
//using OfficeOpenXml.FormulaParsing.Utilities;

//public record UpdateDetailsCommand : IRequest<Result<string>>
//{
//    public int Id { get; set; }
//    public string UserName { get; set; } = string.Empty;
//    public string Email { get; set; } = string.Empty;
//    public string Role { get; set; } = string.Empty;
//    public string Image { get; set; } = string.Empty;
//}

//public class UpdateDetailsCommandHandler : IRequestHandler<UpdateDetailsCommand, Result<string>>
//{
//    private readonly IApplicationDbContext _context;

//    public UpdateDetailsCommandHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<string>> Handle(UpdateDetailsCommand request, CancellationToken cancellationToken)
//    {
//        var adminUser = await _context.UserDetails.FindAsync(new object[] { request.Id }, cancellationToken);

//        if (adminUser == null)
//        {
//            return Result<string>.Failure(StatusCodes.Status404NotFound, AppMessages.AdminNotFound);
//        }

//        // If the provided role matches the existing role, proceed with updating details
//        if (adminUser.Role == request.Role)
//        {
//            adminUser.FullName = request.UserName;
//            // If role is NOT "Sub-Admin", update both Name and Email
//            if (adminUser.Role.ToLower() == nameof(Roles.Admin).ToLower())
//            {
//                adminUser.EmailAddress = request.Email;
//            }
//            adminUser.ProfilePicture = request.Image;
//            adminUser.LastModified = DateTime.UtcNow;
//            await _context.SaveChangesAsync(cancellationToken);

//            return Result<string>.Success(StatusCodes.Status200OK, AppMessages.AdminUpdated);
//        }

//        // If roles do not match, return an error
//        return Result<string>.Failure(StatusCodes.Status400BadRequest, AppMessages.RoleMismatch);
//    }
//}
