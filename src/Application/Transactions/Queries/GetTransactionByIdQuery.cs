using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;

namespace Escrow.Api.Application.Transactions.Queries;
public record GetTransactionByIdQuery : IRequest<Result<TransactionDTO>>
{
    public int TransactionId { get; init; }
}

public class GetTransactionByIdQueryValidator : AbstractValidator<GetTransactionByIdQuery>
{
    public GetTransactionByIdQueryValidator()
    {
        RuleFor(x => x.TransactionId)
            .GreaterThan(0).WithMessage("Transaction ID must be a positive integer.");
    }
}

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, Result<TransactionDTO>>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TransactionDTO>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        // Validate request
        var validator = new GetTransactionByIdQueryValidator();
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return Result<TransactionDTO>.Failure(400, validationResult.Errors.First().ErrorMessage);
        }

        var transaction = await _context.Transactions
            .Where(t => t.Id == request.TransactionId)
            .Select(t => new TransactionDTO
            {
                Id = t.Id,
                TransactionDateTime = t.TransactionDateTime,
                TransactionAmount = t.TransactionAmount,
                TransactionType = t.TransactionType ?? string.Empty,
                FromPayee = t.FromPayee ?? string.Empty,
                ToRecipient = t.ToRecipient ?? string.Empty,
                ContractId = t.ContractId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (transaction == null)
        {
            return Result<TransactionDTO>.Failure(404, "Transaction not found.");
        }

        return Result<TransactionDTO>.Success(200, "Transaction retrieved successfully.", transaction);
    }
}
