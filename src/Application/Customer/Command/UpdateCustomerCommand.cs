using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Application.Customer.Commands
{
    public record UpdateCustomerCommand : IRequest<Result<object>>
    {
        public int CustomerId { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string MobileNumber { get; init; } = string.Empty;
        public string EmailAddress { get; init; } = string.Empty;
        public string ProfilePicture { get; init; } = string.Empty;
    }

    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateCustomerCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<object>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            // Get the current language (defaults to English if none provided)
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            var result = await _context.UserDetails
                .Where(c => c.Id == request.CustomerId || c.EmailAddress == request.EmailAddress)
                .Select(c => new
                {
                    IsTargetCustomer = c.Id == request.CustomerId,
                    IsEmailConflict = c.EmailAddress == request.EmailAddress && c.Id != request.CustomerId,
                    Customer = c
                })
                .ToListAsync(cancellationToken);

            var customerEntry = result.FirstOrDefault(r => r.IsTargetCustomer);
            var hasEmailConflict = result.Any(r => r.IsEmailConflict);

            if (customerEntry == null)
            {
                // Return localized failure message if customer not found
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("Customernotfound", language));
            }

            if (hasEmailConflict)
            {
                // Return localized failure message if email already exists
                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("EmailExists", language));
            }

            var customer = customerEntry.Customer;

            customer.FullName = request.FullName;
            customer.PhoneNumber = request.MobileNumber;
            customer.EmailAddress = request.EmailAddress;
            customer.ProfilePicture = request.ProfilePicture;
            customer.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Return localized success message for customer update
            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("Customerupdated", language));
        }
    }
}


















































//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using MediatR;
//using Escrow.Api.Application.Common.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Http;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Entities.UserPanel;
//using Escrow.Api.Domain.Enums;
//using Escrow.Api.Application.Common.Constants;

//namespace Escrow.Api.Application.Customer.Commands
//{
//    public record UpdateCustomerCommand : IRequest<Result<object>>
//    {
//        public int CustomerId { get; init; }
//        public string FullName { get; init; } = string.Empty;
//        public string MobileNumber { get; init; } = string.Empty;
//        public string EmailAddress { get; init; } = string.Empty;
//        public string ProfilePicture { get; init; } = string.Empty;
//    }

//    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<object>>
//    {
//        private readonly IApplicationDbContext _context;

//        public UpdateCustomerCommandHandler(IApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<Result<object>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
//        {
//            var result = await _context.UserDetails
//                .Where(c => c.Id == request.CustomerId || c.EmailAddress == request.EmailAddress)
//                .Select(c => new
//                {
//                    IsTargetCustomer = c.Id == request.CustomerId,
//                    IsEmailConflict = c.EmailAddress == request.EmailAddress && c.Id != request.CustomerId,
//                    Customer = c
//                })
//                .ToListAsync(cancellationToken);

//            var customerEntry = result.FirstOrDefault(r => r.IsTargetCustomer);
//            var hasEmailConflict = result.Any(r => r.IsEmailConflict);

//            if (customerEntry == null)
//            {
//                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Customernotfound);
//            }

//            if (hasEmailConflict)
//            {
//                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.EmailExists);
//            }

//            var customer = customerEntry.Customer;

//            customer.FullName = request.FullName;
//            customer.PhoneNumber = request.MobileNumber;
//            customer.EmailAddress = request.EmailAddress;
//            customer.ProfilePicture = request.ProfilePicture;
//            customer.LastModified = DateTime.UtcNow;

//            await _context.SaveChangesAsync(cancellationToken);

//            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Customerupdated);
//        }

//    }
//}
