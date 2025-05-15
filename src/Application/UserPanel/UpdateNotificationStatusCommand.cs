using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.UserPanel.Commands;
public class UpdateNotificationStatusCommand : IRequest<Result<object>>

{
    // NOTHING HERE
}

public class UpdateNotificationStatusCommandHandler : IRequestHandler<UpdateNotificationStatusCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public UpdateNotificationStatusCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<object>> Handle(UpdateNotificationStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.GetUserId().ToInt();
        var userDetail = await _context.UserDetails
            .FirstOrDefaultAsync(n => n.Id == userId, cancellationToken);

        if (userDetail == null)
        {
            return Result<object>.Failure(StatusCodes.Status404NotFound, "User not found.");
        }

        //userDetail.IsNotified = request.IsNotified;
        //userDetail.IsNotified = userDetail.IsNotified == null ? true : !userDetail.IsNotified;
        userDetail.IsNotified = !(userDetail.IsNotified ?? false);
        userDetail.LastModifiedBy = userId.ToString();
        userDetail.LastModified = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<object>.Success(StatusCodes.Status200OK, "Notification status updated successfully.", new { userId = userId, IsNotified = userDetail.IsNotified });
    }
}
