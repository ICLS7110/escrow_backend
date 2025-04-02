using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;

namespace Escrow.Api.Application.Transactions.Queries;
public record SearchTransactionsQuery : IRequest<PaginatedList<TransactionDTO>>
{
    public string? Keyword { get; init; }
    public string? TransactionType { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}

public class SearchTransactionsQueryValidator : AbstractValidator<SearchTransactionsQuery>
{
    public SearchTransactionsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than zero.");
        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Page size must be greater than zero.");
    }
}

public class SearchTransactionsQueryHandler : IRequestHandler<SearchTransactionsQuery, PaginatedList<TransactionDTO>>
{
    private readonly IApplicationDbContext _context;

    public SearchTransactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PaginatedList<TransactionDTO>> Handle(SearchTransactionsQuery request, CancellationToken cancellationToken)
    {
        var validator = new SearchTransactionsQueryValidator();
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid input.");
        }

        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        var query = _context.Transactions.AsQueryable();

        // 🔍 Filtering logic
        if (!string.IsNullOrEmpty(request.Keyword))
        {
            query = query.Where(t => t.TransactionType != null && t.TransactionType.Contains(request.Keyword));
        }

        if (!string.IsNullOrEmpty(request.TransactionType))
        {
            query = query.Where(t => t.TransactionType == request.TransactionType);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(t => t.TransactionDateTime >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(t => t.TransactionDateTime <= request.EndDate.Value);
        }

        // 📌 Apply Pagination
        var paginatedTransactions = await query
            .OrderByDescending(t => t.TransactionDateTime)
            .Select(t => new TransactionDTO
            {
                Id = t.Id,
                TransactionDateTime = t.TransactionDateTime,
                TransactionAmount = t.TransactionAmount,
                TransactionType = t.TransactionType ?? "N/A",
                FromPayee = t.FromPayee ?? "N/A",
                ToRecipient = t.ToRecipient ?? "N/A",
                ContractId = t.ContractId
            })
            .PaginatedListAsync(pageNumber, pageSize);

        return paginatedTransactions;
    }
}
