

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.Commissions;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Commissions.Commands;

public record UpsertCommissionRateCommand(
    int Id,
    decimal CommissionRate,
    bool AppliedGlobally,
    string TransactionType,
    decimal? MinAmount,
    decimal TaxRate
) : IRequest<Result<object>>;

public class UpsertCommissionRateCommandValidator : AbstractValidator<UpsertCommissionRateCommand>
{
    public UpsertCommissionRateCommandValidator()
    {
        RuleFor(x => x.CommissionRate).GreaterThan(0).WithMessage("Commission rate must be greater than zero.");
        RuleFor(x => x.TransactionType).NotEmpty().WithMessage("Transaction type is required.");
        RuleFor(x => x.TaxRate).GreaterThanOrEqualTo(0).WithMessage("Tax rate cannot be negative.");
    }
}

public class UpsertCommissionRateCommandHandler : IRequestHandler<UpsertCommissionRateCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;

    public UpsertCommissionRateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object>> Handle(UpsertCommissionRateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var commission = await _context.CommissionMasters.FindAsync(new object[] { request.Id }, cancellationToken);

            if (commission == null)
            {
                // INSERT logic (Upsert)
                var newCommission = new CommissionMaster
                {
                    Id = request.Id,
                    CommissionRate = request.CommissionRate,
                    AppliedGlobally = request.AppliedGlobally,
                    TransactionType = request.TransactionType,
                    TaxRate = request.TaxRate,
                    MinAmount = request.MinAmount?.ToString(),
                    Created = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                };

                _context.CommissionMasters.Add(newCommission);
                await _context.SaveChangesAsync(cancellationToken);

                return Result<object>.Success(StatusCodes.Status201Created, "Commission created successfully.", newCommission);
            }
            else
            {
                // UPDATE logic
                commission.CommissionRate = request.CommissionRate;
                commission.AppliedGlobally = request.AppliedGlobally;
                commission.TransactionType = request.TransactionType;
                commission.TaxRate = request.TaxRate;
                commission.MinAmount = request.MinAmount?.ToString();
                commission.LastModified = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                return Result<object>.Success(StatusCodes.Status200OK, "Commission updated successfully.", commission);
            }
        }
        catch (Exception ex)
        {
            return Result<object>.Failure(StatusCodes.Status500InternalServerError, $"Unexpected error: {ex.Message}");
        }
    }
}
