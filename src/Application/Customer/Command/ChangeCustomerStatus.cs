using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ChangeCustomerStatusCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(ChangeCustomerStatusCommand request, CancellationToken cancellationToken)
        {
            var customer = await _context.UserDetails
                .Where(x => x.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (customer == null)
            {
                return Result<bool>.Failure(StatusCodes.Status404NotFound, "Customer not found.");
            }

            customer.IsActive = !customer.IsActive;
            customer.LastModified = DateTime.UtcNow;
            customer.LastModifiedBy = request.UpdatedBy.ToString();

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(StatusCodes.Status200OK, "Customer status updated successfully.", true);
        }
    }
}
