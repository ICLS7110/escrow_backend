using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Customer.Command
{
    public class ChangeCustomerStatusCommand : IRequest<Result<bool>>
    {
        public int Id { get; init; }
        public int UpdatedBy { get; init; }
    }

    // Handler to process the command
    public class ChangeCustomerStatusCommandHandler : IRequestHandler<ChangeCustomerStatusCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangeCustomerStatusCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<bool>> Handle(ChangeCustomerStatusCommand request, CancellationToken cancellationToken)
        {
            // Get the current language (defaults to English if none provided)
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            // Find the customer in the database
            var customer = await _context.UserDetails
                .Where(x => x.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (customer == null)
            {
                // Return localized failure message if customer not found
                return Result<bool>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("Customernotfound", language));
            }

            // Toggle the customer's active status
            customer.IsActive = !customer.IsActive;
            customer.LastModified = DateTime.UtcNow;
            customer.LastModifiedBy = request.UpdatedBy.ToString();

            // Save changes to the database
            await _context.SaveChangesAsync(cancellationToken);

            // Return localized success message for customer status change
            return Result<bool>.Success(StatusCodes.Status200OK, AppMessages.Get("Customerstatus", language), true);
        }
    }
}











































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.Customer.Command
//{

//    public class ChangeCustomerStatusCommand : IRequest<Result<bool>>
//    {
//        public int Id { get; init; }
//        public int UpdatedBy { get; init; }
//    }

//    // Handler to process the command
//    public class ChangeCustomerStatusCommandHandler : IRequestHandler<ChangeCustomerStatusCommand, Result<bool>>
//    {
//        private readonly IApplicationDbContext _context;

//        public ChangeCustomerStatusCommandHandler(IApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<Result<bool>> Handle(ChangeCustomerStatusCommand request, CancellationToken cancellationToken)
//        {
//            var customer = await _context.UserDetails
//                .Where(x => x.Id == request.Id)
//                .FirstOrDefaultAsync(cancellationToken);

//            if (customer == null)
//            {
//                return Result<bool>.Failure(StatusCodes.Status404NotFound, AppMessages.Customernotfound);
//            }

//            customer.IsActive = !customer.IsActive;
//            customer.LastModified = DateTime.UtcNow;
//            customer.LastModifiedBy = request.UpdatedBy.ToString();

//            await _context.SaveChangesAsync(cancellationToken);

//            return Result<bool>.Success(StatusCodes.Status200OK, AppMessages.Customerstatus, true);
//        }
//    }
//}
