using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // Import for StatusCodes

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

        public AdminChangeContractStatusCommandHandler(IApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<Result<bool>> Handle(AdminChangeContractStatusCommand request, CancellationToken cancellationToken)
        {
            int userId = _jwtService.GetUserId().ToInt(); // Get authenticated user ID

            // Fetch contract
            var contract = await _context.ContractDetails
                .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

            if (contract == null)
                return Result<bool>.Failure(StatusCodes.Status404NotFound, "Contract not found.");

            // Toggle IsActive status
            contract.IsActive = contract.IsActive == null ? true : !contract.IsActive;
            contract.LastModified = DateTime.UtcNow;
            contract.LastModifiedBy = userId.ToString(); // Store modifier ID

            await _context.SaveChangesAsync(cancellationToken);

            string message = contract.IsActive.HasValue ? "Contract activated successfully." : "Contract deactivated successfully.";
            return Result<bool>.Success(StatusCodes.Status200OK, message, true);
        }
    }
}
