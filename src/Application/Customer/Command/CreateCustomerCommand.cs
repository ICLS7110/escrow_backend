using System;
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
    public record CreateCustomerCommand : IRequest<Result<object>>
    {
        public string FullName { get; init; } = string.Empty;
        public string MobileNumber { get; init; } = string.Empty;
        public string EmailAddress { get; init; } = string.Empty;
        public string ProfilePicture { get; init; } = string.Empty;
    }

    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateCustomerCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<object>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            // Get the current language (defaults to English if none provided)
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            // Check if email or mobile number already exists
            var existingCustomer = await _context.UserDetails
                .FirstOrDefaultAsync(c => c.EmailAddress == request.EmailAddress || c.PhoneNumber == request.MobileNumber, cancellationToken);

            if (existingCustomer != null)
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("EmailOrMobileNumberalreadyexists", language));
            }

            var newCustomer = new UserDetail
            {
                UserId = Guid.NewGuid().ToString(),
                FullName = request.FullName,
                PhoneNumber = request.MobileNumber,
                EmailAddress = request.EmailAddress,
                ProfilePicture = request.ProfilePicture,
                IsActive = true,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Role = nameof(Roles.User)
            };

            _context.UserDetails.Add(newCustomer);
            await _context.SaveChangesAsync(cancellationToken);

            // Return a localized success message
            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("Customercreated", language), new { CustomerId = newCustomer.Id });
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
//    public record CreateCustomerCommand : IRequest<Result<object>>
//    {
//        public string FullName { get; init; } = string.Empty;
//        public string MobileNumber { get; init; } = string.Empty;
//        public string EmailAddress { get; init; } = string.Empty;
//        public string ProfilePicture { get; init; } = string.Empty;
//    }

//    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<object>>
//    {
//        private readonly IApplicationDbContext _context;

//        public CreateCustomerCommandHandler(IApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<Result<object>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
//        {
//            // Check if email already exists
//            var existingCustomer = await _context.UserDetails
//                .FirstOrDefaultAsync(c => c.EmailAddress == request.EmailAddress || c.PhoneNumber == request.MobileNumber, cancellationToken);

//            if (existingCustomer != null)
//            {
//                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.EmailOrMobileNumberalreadyexists);
//            }

//            var newCustomer = new UserDetail
//            {
//                UserId= Guid.NewGuid().ToString(),
//                FullName = request.FullName,
//                PhoneNumber = request.MobileNumber,
//                EmailAddress = request.EmailAddress,
//                ProfilePicture = request.ProfilePicture,
//                IsActive=true,
//                Created = DateTime.UtcNow,
//                LastModified = DateTime.UtcNow,
//                Role= nameof(Roles.User)
//            };

//            _context.UserDetails.Add(newCustomer);
//            await _context.SaveChangesAsync(cancellationToken);

//            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Customercreated, new { CustomerId = newCustomer.Id });
//        }
//    }
//}
