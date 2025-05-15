using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.DTOs;

namespace Escrow.Api.Application.Customers.Commands
{
    public record DeleteCustomerCommand(int CustomerId, int DeletedBy) : IRequest<Result<object>>;

    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteCustomerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<object>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.Id == request.CustomerId, cancellationToken);

            if (customer == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, "Customer not found.");
            }
            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            customer.DeletedBy = request.DeletedBy;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, "Customer deleted successfully.");
        }
    }
}
