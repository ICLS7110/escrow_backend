using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.UserPanel.Commands;

public class StoreDeviceTokenCommand : IRequest<Result<object>>
{
    public string? DeviceToken { get; set; }
}

public class StoreDeviceTokenCommandHandler : IRequestHandler<StoreDeviceTokenCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StoreDeviceTokenCommandHandler(IApplicationDbContext context, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<object>> Handle(StoreDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        // Fetch the UserId from the JWT token
        int? userId = _jwtService.GetUserId()?.ToInt();

        if (userId == null || userId <= 0)
        {
            return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("InvalidUserId", language));
        }

        // Find the user in the database using the UserId
        var user = await _context.UserDetails.FindAsync(userId.Value);
        if (user == null)
        {
            return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("UserNotFound", language));
        }

        // Update the user's DeviceToken
        user.DeviceToken = request.DeviceToken;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("DeviceTokenUpdatedSuccessfully", language), new());
    }
}


































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.UserPanel.Commands;
//public class StoreDeviceTokenCommand : IRequest<Result<object>>
//{
//    public string? DeviceToken { get; set; }
//}

//public class StoreDeviceTokenCommandHandler : IRequestHandler<StoreDeviceTokenCommand, Result<object>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;

//    public StoreDeviceTokenCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//    {
//        _context = context;
//        _jwtService = jwtService;
//    }

//    public async Task<Result<object>> Handle(StoreDeviceTokenCommand request, CancellationToken cancellationToken)
//    {
//        // Fetch the UserId from the JWT token
//        int? userId = _jwtService.GetUserId()?.ToInt();

//        if (userId == null || userId <= 0)
//        {
//            return Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid user ID.");
//        }

//        // Find the user in the database using the UserId
//        var user = await _context.UserDetails.FindAsync(userId.Value);
//        if (user == null)
//        {
//            return Result<object>.Failure(StatusCodes.Status404NotFound, "User not found.");
//        }

//        // Update the user's DeviceToken
//        user.DeviceToken = request.DeviceToken;
//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<object>.Success(StatusCodes.Status200OK, "Device token updated successfully.", new());
//    }
//}

