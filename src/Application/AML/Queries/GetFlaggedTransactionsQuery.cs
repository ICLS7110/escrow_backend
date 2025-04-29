using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Domain.Entities.UserPanel;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.AML.Queries;

public record GetFlaggedTransactionsQuery : IRequest<PaginatedList<FlaggedTransactionDto>>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Status { get; init; } // Pending, Approved, Held, Flagged
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}

public class GetFlaggedTransactionsQueryHandler : IRequestHandler<GetFlaggedTransactionsQuery, PaginatedList<FlaggedTransactionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFlaggedTransactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<FlaggedTransactionDto>> Handle(GetFlaggedTransactionsQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        // ✅ Use JOIN instead of navigation property
        var query = from transaction in _context.AMLFlaggedTransactions.AsNoTracking()
                    join user in _context.UserDetails.AsNoTracking()
                    on transaction.UserId equals user.UserId into userGroup
                    from user in userGroup.DefaultIfEmpty() // ✅ LEFT JOIN to get User details
                    where
    (!request.StartDate.HasValue || transaction.Created >= DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc)) &&
    (!request.EndDate.HasValue || transaction.Created <= DateTime.SpecifyKind(request.EndDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)) &&
    (string.IsNullOrWhiteSpace(request.Status) || transaction.Status == request.Status)

                    orderby transaction.Created descending
                    select new FlaggedTransactionDto
                    {
                        Id = transaction.Id.ToString(),
                        TransactionId = transaction.TransactionId,
                        UserId = transaction.UserId,
                        Amount = transaction.Amount,
                        Status = transaction.Status,
                        Reason = transaction.RiskReason,
                        CreatedAt = transaction.Created.ToString(),

                        // ✅ Fetch User Details
                        User = user != null ? new UserDetail
                        {
                            UserId = user.UserId,
                            FullName = user.FullName,
                            EmailAddress = user.EmailAddress,
                            PhoneNumber = user.PhoneNumber,
                            Gender = user.Gender,
                            DateOfBirth = user.DateOfBirth,
                            BusinessManagerName = user.BusinessManagerName,
                            BusinessEmail = user.BusinessEmail,
                            CompanyEmail = user.CompanyEmail,
                            VatId = user.VatId,
                            BusinessProof = user.BusinessProof,
                            ProfilePicture = user.ProfilePicture,
                            AccountType = user.AccountType,
                            IsProfileCompleted = user.IsProfileCompleted,
                            IsDeleted = user.IsDeleted,
                            IsActive = user.IsActive ?? false,
                            LoginMethod = user.LoginMethod
                        } : null
                    };

        // ✅ Fetch paginated data in a single query
        var totalRecords = await query.CountAsync(cancellationToken);
        var transactions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<FlaggedTransactionDto>(transactions, totalRecords, pageNumber, pageSize);
    }
}
