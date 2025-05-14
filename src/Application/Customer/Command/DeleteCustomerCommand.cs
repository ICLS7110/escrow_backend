using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Application.Customers.Commands
{
    public record DeleteCustomerCommand(int CustomerId, int DeletedBy) : IRequest<Result<object>>;

    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteCustomerCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<object>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            // Get the current language (defaults to English if none provided)
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            var customer = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.Id == request.CustomerId, cancellationToken);

            if (customer == null)
            {
                // Return localized failure message if customer not found
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("Customernotfound", language));
            }

            // Mark the customer as deleted
            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            customer.DeletedBy = request.DeletedBy;

            // Save the changes to the database
            await _context.SaveChangesAsync(cancellationToken);

            // Return localized success message for customer deletion
            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("Customerdeleted", language));
        }
    }
}









































//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using MediatR;
//using Escrow.Api.Application.Common.Interfaces;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Application.Common.Constants;

//namespace Escrow.Api.Application.Customers.Commands
//{
//    public record DeleteCustomerCommand(int CustomerId, int DeletedBy) : IRequest<Result<object>>;

//    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<object>>
//    {
//        private readonly IApplicationDbContext _context;

//        public DeleteCustomerCommandHandler(IApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<Result<object>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
//        {
//            var customer = await _context.UserDetails
//                .FirstOrDefaultAsync(u => u.Id == request.CustomerId, cancellationToken);

//            if (customer == null)
//            {
//                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Customernotfound);
//            }
//            customer.IsDeleted = true;
//            customer.DeletedAt = DateTime.UtcNow;
//            customer.DeletedBy = request.DeletedBy;

//            await _context.SaveChangesAsync(cancellationToken);

//            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Customerdeleted);
//        }
//    }
//}
