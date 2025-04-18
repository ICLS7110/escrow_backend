using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Commissions.Commands;
public record UpdateCommissionRateCommand(int Id, decimal CommissionRate, bool AppliedGlobally, string TransactionType, decimal TaxRate) : IRequest<Result<int>>;

public class UpdateCommissionRateCommandValidator : AbstractValidator<UpdateCommissionRateCommand>
{
    public UpdateCommissionRateCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Invalid commission ID.");
        RuleFor(x => x.CommissionRate).GreaterThan(0).WithMessage("Commission rate must be greater than zero.");
        RuleFor(x => x.TransactionType).NotEmpty().WithMessage("Transaction type is required.");
        RuleFor(x => x.TaxRate).GreaterThanOrEqualTo(0).WithMessage("Tax rate cannot be negative.");
    }
}

public class UpdateCommissionRateCommandHandler : IRequestHandler<UpdateCommissionRateCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public UpdateCommissionRateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(UpdateCommissionRateCommand request, CancellationToken cancellationToken)
    {
        var commission = await _context.CommissionMasters.FindAsync(new object[] { request.Id }, cancellationToken);

        if (commission == null)
            return Result<int>.Failure(404, "Commission record not found.");

        commission.CommissionRate = request.CommissionRate;
        commission.AppliedGlobally = request.AppliedGlobally;
        commission.TransactionType = request.TransactionType;
        commission.TaxRate = request.TaxRate;
        commission.LastModified = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(StatusCodes.Status200OK, "Commission rate updated successfully.", commission.Id);
    }
}
