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

namespace Escrow.Api.Application.Customer.Commands
{
    public record CreateCustomerCommand : IRequest<Result<object>>
    {
        public string FullName { get; init; } = string.Empty;
        public string MobileNumber { get; init; } = string.Empty;
        public string EmailAddress { get; init; } = string.Empty;
    }

    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;

        public CreateCustomerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<object>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            // Check if email already exists
            var existingCustomer = await _context.UserDetails
                .FirstOrDefaultAsync(c => c.EmailAddress == request.EmailAddress, cancellationToken);

            if (existingCustomer != null)
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, "Email already exists.");
            }

            var newCustomer = new UserDetail
            {
                FullName = request.FullName,
                PhoneNumber = request.MobileNumber,
                EmailAddress = request.EmailAddress,
                IsActive=true,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            _context.UserDetails.Add(newCustomer);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, "Customer created successfully.", new { CustomerId = newCustomer.Id });
        }
    }
}
