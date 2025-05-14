namespace Escrow.Api.Application.BankDetails.Commands;

using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

public record DeleteBankDetailCommand(int Id) : IRequest<Result<int>>;

public class DeleteBankDetailCommandHandler : IRequestHandler<DeleteBankDetailCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeleteBankDetailCommandHandler(IApplicationDbContext context, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<int>> Handle(DeleteBankDetailCommand request, CancellationToken cancellationToken)
    {
        // Get the current language from HttpContext
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        // Retrieve user ID from JWT service
        var userId = _jwtService.GetUserId().ToInt();

        // Fetch bank details for the user
        var entity = await _context.BankDetails
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserDetailId == userId, cancellationToken);

        // If entity is not found, return an appropriate error message
        if (entity == null)
        {
            return Result<int>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("BankDetailsNotFound", language));
        }

        // Mark the bank detail as deleted
        entity.RecordState = RecordState.Deleted;
        entity.DeletedAt = DateTimeOffset.UtcNow;
        entity.DeletedBy = userId;

        // Update the entity in the database
        _context.BankDetails.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Return success message
        return Result<int>.Success(StatusCodes.Status200OK, AppMessages.Get("Success", language));
    }
}








































//namespace Escrow.Api.Application.BankDetails.Commands;

//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Enums;
//using Microsoft.AspNetCore.Http;

//public record DeleteBankDetailCommand(int Id): IRequest<Result<int>>;

//public class DeleteBankDetailCommandHandler : IRequestHandler<DeleteBankDetailCommand, Result<int>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;
//    public DeleteBankDetailCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//    {
//        _context = context;
//        _jwtService = jwtService;
//    }

//    public async Task<Result<int>> Handle(DeleteBankDetailCommand request, CancellationToken cancellationToken)
//    {
//        var userid= _jwtService.GetUserId().ToInt();
//        var entity = await _context.BankDetails
//            .FirstOrDefaultAsync(x => x.Id==request.Id && x.UserDetailId==userid);

//        if (entity == null)
//            return Result<int>.Failure(StatusCodes.Status404NotFound, AppMessages.BankDetailsNotFound);

//        entity.RecordState = RecordState.Deleted;
//        entity.DeletedAt= DateTimeOffset.UtcNow;
//        entity.DeletedBy = userid;
//        _context.BankDetails.Update(entity);
//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<int>.Success(StatusCodes.Status200OK, AppMessages.Success);
//    }  
//}
