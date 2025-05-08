using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Enums;

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
    private readonly IJwtService _jwtService;

    public GetTransactionByIdQueryHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
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
        var userId = _jwtService.GetUserId().ToInt();

        var userRole = await _context.UserDetails
          .Where(u => u.Id == userId)
          .Select(u => u.Role)
          .FirstOrDefaultAsync(cancellationToken);

        // Step 1: Filter Transactions first
        var transactionQuery = _context.Transactions
            .Where(t => t.Id == request.TransactionId);

        // Apply role-based restriction before projecting
        if (userRole == nameof(Roles.User))
        {
            transactionQuery = transactionQuery.Where(t => t.CreatedBy == userId.ToString());
        }

        // Step 2: Perform projection with join
        var transaction = await (
            from t in transactionQuery
            join c in _context.ContractDetails on t.ContractId equals c.Id into tc
            from contract in tc.DefaultIfEmpty()
            select new TransactionDTO
            {
                Id = t.Id,
                TransactionDateTime = t.TransactionDateTime,
                TransactionAmount = t.TransactionAmount,
                TransactionType = t.TransactionType ?? string.Empty,
                FromPayee = t.FromPayee ?? string.Empty,
                ToRecipient = t.ToRecipient ?? string.Empty,
                ContractId = t.ContractId,
                Status = t.Status,
                ContractDetails = contract == null ? null : new ContractDetailsDTO
                {
                    Id = contract.Id,
                    Role = contract.Role,
                    ContractTitle = contract.ContractTitle,
                    ServiceType = contract.ServiceType,
                    ServiceDescription = contract.ServiceDescription,
                    AdditionalNote = contract.AdditionalNote,
                    FeesPaidBy = contract.FeesPaidBy,
                    FeeAmount = contract.FeeAmount,
                    BuyerName = contract.BuyerName,
                    BuyerMobile = contract.BuyerMobile,
                    BuyerId = contract.BuyerDetailsId.ToString(),
                    SellerId = contract.SellerDetailsId.ToString(),
                    SellerName = contract.SellerName,
                    SellerMobile = contract.SellerMobile,
                    CreatedBy = contract.CreatedBy,
                    ContractDoc = contract.ContractDoc,
                    Status = contract.Status,
                    IsActive = contract.IsActive,
                    IsDeleted = contract.IsDeleted,
                    TaxAmount = contract.TaxAmount,
                    EscrowTax = contract.EscrowTax,
                    LastModifiedBy = contract.LastModifiedBy,
                    BuyerPayableAmount = contract.BuyerPayableAmount,
                    SellerPayableAmount = contract.SellerPayableAmount,
                    Created = contract.Created,
                    LastModified = contract.LastModified,

                    MileStones = _context.MileStones
                        .Where(m => m.ContractId == contract.Id)
                        .Select(m => new MileStoneDTO
                        {
                            Id = m.Id,
                            Name = m.Name,
                            Amount = m.Amount,
                            DueDate = m.DueDate,
                            Status = m.Status
                        }).ToList()
                }
            })
            .FirstOrDefaultAsync(cancellationToken);


        if (transaction == null)
        {
            return Result<TransactionDTO>.Failure(404, "Transaction not found.");
        }

        return Result<TransactionDTO>.Success(200, "Transaction retrieved successfully.", transaction);
    }

}
