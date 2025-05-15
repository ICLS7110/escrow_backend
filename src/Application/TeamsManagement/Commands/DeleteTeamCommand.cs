using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.TeamsManagement.Commands;

public class DeleteTeamCommand : IRequest<Result<bool>>
{
    public int TeamId { get; set; }
}

public class DeleteTeamHandler : IRequestHandler<DeleteTeamCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeleteTeamHandler(
        IApplicationDbContext context,
        IJwtService jwtService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<bool>> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        var userId = _jwtService.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<bool>.Failure(StatusCodes.Status401Unauthorized, AppMessages.Get("Unauthorized", language));
        }

        var team = await _context.TeamMembers
            .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

        if (team == null)
        {
            return Result<bool>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("TeamNotFoundOrUnauthorized", language));
        }

        team.IsDeleted = true;
        team.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(StatusCodes.Status200OK, AppMessages.Get("TeamDeletedSuccessfully", language), true);
    }
}





































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.TeamsManagement.Commands;

//public class DeleteTeamCommand : IRequest<Result<bool>>
//{
//    public int TeamId { get; set; }
//}

//public class DeleteTeamHandler : IRequestHandler<DeleteTeamCommand, Result<bool>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;

//    public DeleteTeamHandler(IApplicationDbContext context, IJwtService jwtService)
//    {
//        _context = context;
//        _jwtService = jwtService;
//    }

//    public async Task<Result<bool>> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
//    {

//        var userId = _jwtService.GetUserId();
//        if (userId == null)
//        {
//            return Result<bool>.Failure(StatusCodes.Status401Unauthorized, "Unauthorized user.");
//        }

//        var team = await _context.TeamMembers
//            .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

//        if (team == null)
//        {
//            return Result<bool>.Failure(StatusCodes.Status404NotFound, "Team not found or you do not have permission to delete it.");
//        }

//        team.IsDeleted = true;
//        team.DeletedAt = DateTime.UtcNow;
//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<bool>.Success(StatusCodes.Status200OK, "Team deleted successfully.", true);
//    }
//}
