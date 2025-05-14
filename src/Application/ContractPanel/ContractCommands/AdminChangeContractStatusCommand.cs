using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Application.Common.Constants; // Import for StatusCodes

namespace Escrow.Api.Application.ContractPanel.ContractCommands
{
    public class AdminChangeContractStatusCommand : IRequest<Result<bool>>
    {
        public int ContractId { get; set; }
    }

    public class AdminChangeContractStatusCommandHandler : IRequestHandler<AdminChangeContractStatusCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminChangeContractStatusCommandHandler(IApplicationDbContext context, IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<bool>> Handle(AdminChangeContractStatusCommand request, CancellationToken cancellationToken)
        {
            // Get the current language (defaults to English if none provided)
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            int userId = _jwtService.GetUserId().ToInt(); // Get authenticated user ID

            // Fetch contract
            var contract = await _context.ContractDetails
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
            {
                // Return localized failure message if contract not found
                return Result<bool>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("ContractNotFound", language));
            }

            // Toggle IsActive status
            contract.IsActive = contract.IsActive == null ? true : !contract.IsActive;
            contract.LastModified = DateTime.UtcNow;
            contract.LastModifiedBy = contract.LastModifiedBy == null ? userId.ToString() : contract.LastModifiedBy;

            await _context.SaveChangesAsync(cancellationToken);

            // Return localized success message based on contract status
            string message = contract.IsActive.HasValue ? AppMessages.Get("Contractactivated", language) : AppMessages.Get("Contractdeactivated", language);
            return Result<bool>.Success(StatusCodes.Status200OK, message, true);
        }
    }
}









































//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Http;
//using Escrow.Api.Application.Common.Constants; // Import for StatusCodes

//namespace Escrow.Api.Application.ContractPanel.ContractCommands
//{
//    public class AdminChangeContractStatusCommand : IRequest<Result<bool>>
//    {
//        public int ContractId { get; set; }
//    }

//    public class AdminChangeContractStatusCommandHandler : IRequestHandler<AdminChangeContractStatusCommand, Result<bool>>
//    {
//        private readonly IApplicationDbContext _context;
//        private readonly IJwtService _jwtService;

//        public AdminChangeContractStatusCommandHandler(IApplicationDbContext context, IJwtService jwtService)
//        {
//            _context = context;
//            _jwtService = jwtService;
//        }

//        public async Task<Result<bool>> Handle(AdminChangeContractStatusCommand request, CancellationToken cancellationToken)
//        {
//            int userId = _jwtService.GetUserId().ToInt(); // Get authenticated user ID

//            // Fetch contract
//            var contract = await _context.ContractDetails
//                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

//            if (contract == null)
//                return Result<bool>.Failure(StatusCodes.Status404NotFound, AppMessages.ContractNotFound);

//            // Toggle IsActive status
//            contract.IsActive = contract.IsActive == null ? true : !contract.IsActive;
//            contract.LastModified = DateTime.UtcNow;
//            contract.LastModifiedBy = contract.LastModifiedBy == null ? userId.ToString() : contract.LastModifiedBy;

//            await _context.SaveChangesAsync(cancellationToken);

//            string message = contract.IsActive.HasValue ? AppMessages.Contractactivated : AppMessages.Contractdeactivated;
//            return Result<bool>.Success(StatusCodes.Status200OK, message, true);
//        }
//    }
//}
