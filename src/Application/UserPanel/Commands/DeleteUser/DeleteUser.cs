using Ardalis.GuardClauses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Events.UserPanel;
using Escrow.Api.Domain.Enums;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Application.UserPanel.Commands.DeleteUser
{
    public record DeleteUserCommand(int Id) : IRequest<Result<int>>;


    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteUserCommandHandler(IApplicationDbContext context, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<int>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            var entity = await _context.UserDetails.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (entity == null)
                return Result<int>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("UserDetailsNotFound", language));

            entity.RecordState = RecordState.Deleted;
            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            entity.DeletedBy = _jwtService.GetUserId().ToInt();

            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(StatusCodes.Status200OK, AppMessages.Get("UserDeletedSuccessfully", language));
        }
    }

}
//public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<int>>
//    {
//        private readonly IApplicationDbContext _context;
//        private readonly IJwtService _jwtService;

//        public DeleteUserCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//        {
//            _context = context;
//            _jwtService = jwtService;
//        }

//        public async Task<Result<int>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
//        {
//            // Fetch user entity
//            var entity = await _context.UserDetails.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
//            if (entity == null)
//                return Result<int>.Failure(StatusCodes.Status404NotFound, "User Details Not Found.");

//            entity.RecordState = RecordState.Deleted;
//            entity.IsDeleted = true;


//            entity.LastModified = DateTime.UtcNow;
//            // ✅ Ensure DeletedBy is set correctly
//            entity.DeletedBy = _jwtService.GetUserId().ToInt();

//            //_context.UserDetails.Update(entity);
//            await _context.SaveChangesAsync(cancellationToken);

//            return Result<int>.Success(StatusCodes.Status200OK, "Success");
//        }
//    }
