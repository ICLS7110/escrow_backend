using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Escrow.Api.Application.AdminAuth.Commands
{
    // Command to update the record state of a sub-admin
    public class ChangeStatusCommand : IRequest<Result<bool>>
    {
        public int Id { get; init; }
        public int UpdatedBy { get; init; }
    }

    // Handler to handle the record state update command
    public class ChangeStatusCommandHandler : IRequestHandler<ChangeStatusCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangeStatusCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<bool>> Handle(ChangeStatusCommand request, CancellationToken cancellationToken)
        {
            // Retrieve the current language from HttpContext
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            var subAdminUser = await _context.UserDetails
                .Where(x => x.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (subAdminUser == null)
            {
                return Result<bool>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("NotFound", language));
            }

            // Toggle the IsActive status of the sub-admin
            subAdminUser.IsActive = !subAdminUser.IsActive;
            subAdminUser.LastModified = DateTime.UtcNow;
            subAdminUser.LastModifiedBy = request.UpdatedBy.ToString();

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(StatusCodes.Status200OK, AppMessages.Get("StatusUpdated", language), true);
        }
    }
}










































//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Enums;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.AdminAuth.Commands;

//// Command to update the record state of a sub-admin
//public class ChangeStatusCommand : IRequest<Result<bool>>
//{
//    public int Id { get; init; }
//    public int UpdatedBy { get; init; }
//}

//// Handler to handle the record state update command
//public class ChangeStatusCommandHandler : IRequestHandler<ChangeStatusCommand, Result<bool>>
//{
//    private readonly IApplicationDbContext _context;

//    public ChangeStatusCommandHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<bool>> Handle(ChangeStatusCommand request, CancellationToken cancellationToken)
//    {
//        var subAdminUser = await _context.UserDetails
//            .Where(x => x.Id == request.Id)
//            .FirstOrDefaultAsync(cancellationToken);

//        if (subAdminUser == null)
//        {
//            return Result<bool>.Failure(StatusCodes.Status404NotFound, AppMessages.NotFound);
//        }

//        subAdminUser.IsActive= subAdminUser.IsActive == true ? false : true;    
//        subAdminUser.LastModified = DateTime.UtcNow;
//        subAdminUser.LastModifiedBy = request.UpdatedBy.ToString();

//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<bool>.Success(StatusCodes.Status200OK, AppMessages.StatusUpdated, true);
//    }
//}
