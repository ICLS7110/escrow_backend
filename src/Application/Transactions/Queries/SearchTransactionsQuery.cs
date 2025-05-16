using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.Transactions.Queries;

public record SearchTransactionsQuery : IRequest<Result<PaginatedList<TransactionDTO>>>
{
    public string? Keyword { get; init; }
    public string? TransactionType { get; init; }
    public string? TransactionStatus { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}

public class SearchTransactionsQueryValidator : AbstractValidator<SearchTransactionsQuery>
{
    public SearchTransactionsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0)
            .WithMessage(AppMessages.Get("PageNumberGreaterThanZero", Language.English));
        RuleFor(x => x.PageSize).GreaterThan(0)
            .WithMessage(AppMessages.Get("PageSizeGreaterThanZero", Language.English));
    }
}

public class SearchTransactionsQueryHandler : IRequestHandler<SearchTransactionsQuery, Result<PaginatedList<TransactionDTO>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SearchTransactionsQueryHandler(
        IApplicationDbContext context,
        IJwtService jwtService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<Result<PaginatedList<TransactionDTO>>> Handle(SearchTransactionsQuery request, CancellationToken cancellationToken)
    {
        // Get language from HttpContextAccessor
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        // Validate request
        var validator = new SearchTransactionsQueryValidator();
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            var errorMsg = validationResult.Errors.FirstOrDefault()?.ErrorMessage ??
                           AppMessages.Get("InvalidRequest", language);
            return Result<PaginatedList<TransactionDTO>>.Failure(StatusCodes.Status400BadRequest, errorMsg);
        }

        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        var query = from t in _context.Transactions
                    join c in _context.ContractDetails on t.ContractId equals c.Id into contractGroup
                    from c in contractGroup.DefaultIfEmpty()
                    select new { Transaction = t, Contract = c };

        var currentUserId = _jwtService.GetUserId().ToInt();

        var userRole = await _context.UserDetails
            .Where(u => u.Id == currentUserId)
            .Select(u => u.Role)
            .FirstOrDefaultAsync(cancellationToken);

        if (userRole == nameof(Roles.User))
        {
            query = query.Where(x => x.Transaction.CreatedBy == currentUserId.ToString());
        }

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            var keywordLower = request.Keyword.ToLower();
            query = query.Where(x =>
                (x.Transaction.TransactionType != null && x.Transaction.TransactionType.ToLower().Contains(keywordLower)) ||
                x.Transaction.Id.ToString().Contains(request.Keyword) ||
                (x.Contract != null && x.Contract.ContractTitle.ToLower().Contains(keywordLower))
            );
        }

        if (!string.IsNullOrEmpty(request.TransactionType))
        {
            query = query.Where(x => x.Transaction.TransactionType == request.TransactionType);
        }

        if (!string.IsNullOrEmpty(request.TransactionStatus))
        {
            //query = query.Where(x => x.Transaction.Status == request.TransactionStatus);

            query = query.Where(x => x.Transaction != null &&
                                     x.Transaction.Status != null &&
                                     x.Transaction.Status.ToString().ToLower().Contains(request.TransactionStatus.ToLower()));

        }

        if (request.StartDate.HasValue)
        {
            var startDateUtc = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(x => x.Transaction.TransactionDateTime >= startDateUtc || x.Transaction.Created >= startDateUtc);
        }

        if (request.EndDate.HasValue)
        {
            var endDate = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            var endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            query = query.Where(x => x.Transaction.TransactionDateTime <= endDateUtc || x.Transaction.Created <= endDateUtc);
        }

        // Calculate total amount for filtered transactions
        var totalAmount = await query.SumAsync(x => (decimal?)x.Transaction.TransactionAmount) ?? 0m;

        var paginatedTransactions = await query
            .OrderByDescending(x => x.Transaction.TransactionDateTime)
            .Select(x => new TransactionDTO
            {
                Id = x.Transaction.Id,
                TransactionDateTime = x.Transaction.TransactionDateTime,
                TransactionAmount = x.Transaction.TransactionAmount,
                TransactionType = x.Transaction.TransactionType ?? "N/A",
                FromPayee = x.Transaction.FromPayee ?? "N/A",
                ToRecipient = x.Transaction.ToRecipient ?? "N/A",
                ContractId = x.Transaction.ContractId,
                TotalAmount = totalAmount,
                Status = x.Transaction.Status,
                ContractDetails = x.Contract == null ? null : new ContractDetailsDTO
                {
                    Id = x.Contract.Id,
                    Role = x.Contract.Role,
                    ContractTitle = x.Contract.ContractTitle,
                    ServiceType = x.Contract.ServiceType,
                    ServiceDescription = x.Contract.ServiceDescription,
                    AdditionalNote = x.Contract.AdditionalNote,
                    FeesPaidBy = x.Contract.FeesPaidBy,
                    FeeAmount = x.Contract.FeeAmount,
                    BuyerName = x.Contract.BuyerName,
                    BuyerMobile = x.Contract.BuyerMobile,
                    BuyerId = x.Contract.BuyerDetailsId.ToString(),
                    SellerId = x.Contract.SellerDetailsId.ToString(),
                    SellerName = x.Contract.SellerName,
                    SellerMobile = x.Contract.SellerMobile,
                    CreatedBy = x.Contract.CreatedBy,
                    ContractDoc = x.Contract.ContractDoc,
                    Status = x.Contract.Status,
                    IsActive = x.Contract.IsActive,
                    IsDeleted = x.Contract.IsDeleted,
                    TaxAmount = x.Contract.TaxAmount,
                    EscrowTax = x.Contract.EscrowTax,
                    LastModifiedBy = x.Contract.LastModifiedBy,
                    BuyerPayableAmount = x.Contract.BuyerPayableAmount,
                    SellerPayableAmount = x.Contract.SellerPayableAmount,
                    Created = x.Contract.Created,
                    LastModified = x.Contract.LastModified,
                    MileStones = _context.MileStones
                        .Where(m => m.ContractId == x.Contract.Id)
                        .Select(m => new MileStoneDTO
                        {
                            Id = m.Id,
                            Name = m.Name,
                            Amount = m.Amount,
                            DueDate = m.DueDate,
                            Status = m.Status
                        }).ToList(),
                    TeamMembers = new System.Collections.Generic.List<TeamDTO>(),
                    InvitationDetails = null
                }
            })
            .PaginatedListAsync(pageNumber, pageSize);

        if (!paginatedTransactions.Items.Any())
        {
            return Result<PaginatedList<TransactionDTO>>.Failure(StatusCodes.Status404NotFound,
                AppMessages.Get("NoTransactionsFound", language));
        }

        return Result<PaginatedList<TransactionDTO>>.Success(StatusCodes.Status200OK,
            AppMessages.Get("TransactionRetrievedSuccessfully", language), paginatedTransactions);
    }
}




















































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Mappings;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.Common.Models.ContractDTOs;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Enums;
//using FluentValidation;
//using MediatR;
//using Microsoft.EntityFrameworkCore;

//namespace Escrow.Api.Application.Transactions.Queries;

//public record SearchTransactionsQuery : IRequest<Result<PaginatedList<TransactionDTO>>>
//{
//    public string? Keyword { get; init; }
//    public string? TransactionType { get; init; }
//    public string? TransactionStatus { get; init; }
//    public DateTime? StartDate { get; init; }
//    public DateTime? EndDate { get; init; }
//    public int? PageNumber { get; init; } = 1;
//    public int? PageSize { get; init; } = 10;
//}

//public class SearchTransactionsQueryValidator : AbstractValidator<SearchTransactionsQuery>
//{
//    public SearchTransactionsQueryValidator()
//    {
//        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than zero.");
//        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Page size must be greater than zero.");
//    }
//}

//public class SearchTransactionsQueryHandler : IRequestHandler<SearchTransactionsQuery, Result<PaginatedList<TransactionDTO>>>
//{
//    private readonly IApplicationDbContext _context;
//    private readonly IJwtService _jwtService;

//    public SearchTransactionsQueryHandler(IApplicationDbContext context, IJwtService jwtService)
//    {
//        _context = context ?? throw new ArgumentNullException(nameof(context));
//        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
//    }
//    public async Task<Result<PaginatedList<TransactionDTO>>> Handle(SearchTransactionsQuery request, CancellationToken cancellationToken)
//    {
//        var validator = new SearchTransactionsQueryValidator();
//        var validationResult = validator.Validate(request);

//        if (!validationResult.IsValid)
//        {
//            return Result<PaginatedList<TransactionDTO>>.Failure(400, validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid request.");
//        }

//        int pageNumber = request.PageNumber ?? 1;
//        int pageSize = request.PageSize ?? 10;

//        var query = from t in _context.Transactions
//                    join c in _context.ContractDetails on t.ContractId equals c.Id into contractGroup
//                    from c in contractGroup.DefaultIfEmpty()
//                    select new { Transaction = t, Contract = c };

//        var currentUserId = _jwtService.GetUserId().ToInt();

//        var userRole = await _context.UserDetails
//            .Where(u => u.Id == currentUserId)
//            .Select(u => u.Role)
//            .FirstOrDefaultAsync(cancellationToken);

//        if (userRole == nameof(Roles.User))
//        {
//            query = query.Where(x => x.Transaction.CreatedBy == currentUserId.ToString());
//        }

//        if (!string.IsNullOrEmpty(request.Keyword))
//        {
//            query = query.Where(x =>
//                (x.Transaction.TransactionType != null && x.Transaction.TransactionType.Contains(request.Keyword)) ||
//                x.Transaction.Id.ToString().Contains(request.Keyword) ||
//                (x.Contract != null && x.Contract.ContractTitle.ToLower().Contains(request.Keyword.ToLower()))
//            );
//        }

//        if (!string.IsNullOrEmpty(request.TransactionType))
//        {
//            query = query.Where(x => x.Transaction.TransactionType == request.TransactionType);
//        }

//        if (!string.IsNullOrEmpty(request.TransactionStatus))
//        {
//            query = query.Where(x => x.Transaction.Status == request.TransactionStatus);
//        }

//        if (request.StartDate.HasValue)
//        {
//            var startDateUtc = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
//            query = query.Where(x => x.Transaction.TransactionDateTime >= startDateUtc || x.Transaction.Created >= startDateUtc);
//        }

//        if (request.EndDate.HasValue)
//        {
//            var endDate = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
//            var endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
//            query = query.Where(x => x.Transaction.TransactionDateTime <= endDateUtc || x.Transaction.Created <= endDateUtc);
//        }

//        // Calculate totalAmount after filters applied
//        var totalAmount = await query.SumAsync(x => (decimal?)x.Transaction.TransactionAmount) ?? 0m;

//        var paginatedTransactions = await query
//            .OrderByDescending(x => x.Transaction.TransactionDateTime)
//            .Select(x => new TransactionDTO
//            {
//                Id = x.Transaction.Id,
//                TransactionDateTime = x.Transaction.TransactionDateTime,
//                TransactionAmount = x.Transaction.TransactionAmount,
//                TransactionType = x.Transaction.TransactionType ?? "N/A",
//                FromPayee = x.Transaction.FromPayee ?? "N/A",
//                ToRecipient = x.Transaction.ToRecipient ?? "N/A",
//                ContractId = x.Transaction.ContractId,
//                TotalAmount = totalAmount,
//                Status = x.Transaction.Status,
//                ContractDetails = x.Contract == null ? null : new ContractDetailsDTO
//                {
//                    Id = x.Contract.Id,
//                    Role = x.Contract.Role,
//                    ContractTitle = x.Contract.ContractTitle,
//                    ServiceType = x.Contract.ServiceType,
//                    ServiceDescription = x.Contract.ServiceDescription,
//                    AdditionalNote = x.Contract.AdditionalNote,
//                    FeesPaidBy = x.Contract.FeesPaidBy,
//                    FeeAmount = x.Contract.FeeAmount,
//                    BuyerName = x.Contract.BuyerName,
//                    BuyerMobile = x.Contract.BuyerMobile,
//                    BuyerId = x.Contract.BuyerDetailsId.ToString(),
//                    SellerId = x.Contract.SellerDetailsId.ToString(),
//                    SellerName = x.Contract.SellerName,
//                    SellerMobile = x.Contract.SellerMobile,
//                    CreatedBy = x.Contract.CreatedBy,
//                    ContractDoc = x.Contract.ContractDoc,
//                    Status = x.Contract.Status,
//                    IsActive = x.Contract.IsActive,
//                    IsDeleted = x.Contract.IsDeleted,
//                    TaxAmount = x.Contract.TaxAmount,
//                    EscrowTax = x.Contract.EscrowTax,
//                    LastModifiedBy = x.Contract.LastModifiedBy,
//                    BuyerPayableAmount = x.Contract.BuyerPayableAmount,
//                    SellerPayableAmount = x.Contract.SellerPayableAmount,
//                    Created = x.Contract.Created,
//                    LastModified = x.Contract.LastModified,
//                    MileStones = _context.MileStones
//                        .Where(m => m.ContractId == x.Contract.Id)
//                        .Select(m => new MileStoneDTO
//                        {
//                            Id = m.Id,
//                            Name = m.Name,
//                            Amount = m.Amount,
//                            DueDate = m.DueDate,
//                            Status = m.Status
//                        }).ToList(),
//                    TeamMembers = new List<TeamDTO>(),
//                    InvitationDetails = null
//                }
//            })
//            .PaginatedListAsync(pageNumber, pageSize);

//        if (!paginatedTransactions.Items.Any())
//        {
//            return Result<PaginatedList<TransactionDTO>>.Failure(404, "No transactions found.");
//        }

//        return Result<PaginatedList<TransactionDTO>>.Success(200, "Transactions retrieved successfully.", paginatedTransactions);
//    }

//    //public async Task<Result<PaginatedList<TransactionDTO>>> Handle(SearchTransactionsQuery request, CancellationToken cancellationToken)
//    //{
//    //    var validator = new SearchTransactionsQueryValidator();
//    //    var validationResult = validator.Validate(request);

//    //    if (!validationResult.IsValid)
//    //    {
//    //        return Result<PaginatedList<TransactionDTO>>.Failure(400, validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid request.");
//    //    }

//    //    int pageNumber = request.PageNumber ?? 1;
//    //    int pageSize = request.PageSize ?? 10;

//    //    var query = from t in _context.Transactions
//    //                join c in _context.ContractDetails on t.ContractId equals c.Id into contractGroup
//    //                from c in contractGroup.DefaultIfEmpty()
//    //                select new { Transaction = t, Contract = c };


//    //    var currentUserId = _jwtService.GetUserId().ToInt();

//    //    var userRole = await _context.UserDetails
//    //        .Where(u => u.Id == currentUserId)
//    //        .Select(u => u.Role)
//    //        .FirstOrDefaultAsync(cancellationToken);

//    //    if (userRole == nameof(Roles.User))
//    //    {
//    //        query = query.Where(t => t.Transaction.CreatedBy == currentUserId.ToString());
//    //    }


//    //    var totalAmount = await query.SumAsync(t => (decimal?)t.Transaction.TransactionAmount) ?? 0m;

//    //    if (!string.IsNullOrEmpty(request.Keyword))
//    //    {
//    //        query = query.Where(x =>
//    //            (x.Transaction.TransactionType != null && x.Transaction.TransactionType.Contains(request.Keyword)) ||
//    //            x.Transaction.Id.ToString().Contains(request.Keyword) ||
//    //            (x.Contract != null && x.Contract.ContractTitle.Contains(request.Keyword))
//    //        );
//    //    }

//    //    if (!string.IsNullOrEmpty(request.TransactionType))
//    //    {
//    //        query = query.Where(t => t.Transaction.TransactionType == request.TransactionType);
//    //    }

//    //    if (!string.IsNullOrEmpty(request.TransactionStatus))
//    //    {
//    //        query = query.Where(t => t.Transaction.Status == request.TransactionStatus);
//    //    }

//    //    if (request.StartDate.HasValue)
//    //    {
//    //        var startDateUtc = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
//    //        query = query.Where(t => t.Transaction.TransactionDateTime >= startDateUtc || t.Transaction.Created >= startDateUtc);
//    //    }

//    //    if (request.EndDate.HasValue)
//    //    {
//    //        var endDate = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
//    //        var endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
//    //        query = query.Where(t => t.Transaction.TransactionDateTime <= endDateUtc || t.Transaction.Created <= endDateUtc);
//    //    }

//    //    var paginatedTransactions = await query
//    //        .OrderByDescending(t => t.Transaction.TransactionDateTime)
//    //        .Select(t => new TransactionDTO
//    //        {
//    //            Id = t.Transaction.Id,
//    //            TransactionDateTime = t.Transaction.TransactionDateTime,
//    //            TransactionAmount = t.Transaction.TransactionAmount,
//    //            TransactionType = t.Transaction.TransactionType ?? "N/A",
//    //            FromPayee = t.Transaction.FromPayee ?? "N/A",
//    //            ToRecipient = t.Transaction.ToRecipient ?? "N/A",
//    //            ContractId = t.Transaction.ContractId,
//    //            TotalAmount = totalAmount,
//    //            Status = t.Transaction.Status,
//    //            ContractDetails = t.Transaction.ContractId != null ? (
//    //                from c in _context.ContractDetails
//    //                where c.Id == t.Transaction.ContractId
//    //                select new ContractDetailsDTO
//    //                {
//    //                    Id = c.Id,
//    //                    Role = c.Role,
//    //                    ContractTitle = c.ContractTitle,
//    //                    ServiceType = c.ServiceType,
//    //                    ServiceDescription = c.ServiceDescription,
//    //                    AdditionalNote = c.AdditionalNote,
//    //                    FeesPaidBy = c.FeesPaidBy,
//    //                    FeeAmount = c.FeeAmount,
//    //                    BuyerName = c.BuyerName,
//    //                    BuyerMobile = c.BuyerMobile,
//    //                    BuyerId = c.BuyerDetailsId.ToString(),
//    //                    SellerId = c.SellerDetailsId.ToString(),
//    //                    SellerName = c.SellerName,
//    //                    SellerMobile = c.SellerMobile,
//    //                    CreatedBy = c.CreatedBy,
//    //                    ContractDoc = c.ContractDoc,
//    //                    Status = c.Status,
//    //                    IsActive = c.IsActive,
//    //                    IsDeleted = c.IsDeleted,
//    //                    TaxAmount = c.TaxAmount,
//    //                    EscrowTax = c.EscrowTax,
//    //                    LastModifiedBy = c.LastModifiedBy,
//    //                    BuyerPayableAmount = c.BuyerPayableAmount,
//    //                    SellerPayableAmount = c.SellerPayableAmount,
//    //                    Created = c.Created,
//    //                    LastModified = c.LastModified,
//    //                    MileStones = _context.MileStones
//    //                        .Where(m => m.ContractId == c.Id)
//    //                        .Select(m => new MileStoneDTO
//    //                        {
//    //                            Id = m.Id,
//    //                            Name = m.Name,
//    //                            Amount = m.Amount,
//    //                            DueDate = m.DueDate,
//    //                            Status = m.Status
//    //                        }).ToList(),
//    //                    TeamMembers = new List<TeamDTO>(),
//    //                    InvitationDetails = null
//    //                }).FirstOrDefault() : null
//    //        })
//    //        .PaginatedListAsync(pageNumber, pageSize);

//    //    if (!paginatedTransactions.Items.Any())
//    //    {
//    //        return Result<PaginatedList<TransactionDTO>>.Failure(404, "No transactions found.");
//    //    }

//    //    return Result<PaginatedList<TransactionDTO>>.Success(200, "Transactions retrieved successfully.", paginatedTransactions);
//    //}
//}
















////public class SearchTransactionsQueryHandler : IRequestHandler<SearchTransactionsQuery, PaginatedList<TransactionDTO>>
////{
////    private readonly IApplicationDbContext _context;
////    private readonly IJwtService _jwtService;

////    public SearchTransactionsQueryHandler(IApplicationDbContext context, IJwtService jwtService)
////    {
////        _context = context ?? throw new ArgumentNullException(nameof(context));
////        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
////    }

////    public async Task<PaginatedList<TransactionDTO>> Handle(SearchTransactionsQuery request, CancellationToken cancellationToken)
////    {
////        var validator = new SearchTransactionsQueryValidator();
////        var validationResult = validator.Validate(request);
////        if (!validationResult.IsValid)
////        {
////            throw new ArgumentException(validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid input.");
////        }

////        int pageNumber = request.PageNumber ?? 1;
////        int pageSize = request.PageSize ?? 10;

////        var query = _context.Transactions.AsQueryable();

////        // 🔐 Get User ID and Role from JWT
////        var currentUserId = _jwtService.GetUserId().ToInt();


////        var userRole = await _context.UserDetails
////          .Where(u => u.Id == currentUserId)
////          .Select(u => u.Role)
////          .FirstOrDefaultAsync(cancellationToken);


////        // 👤 If user is 'User', restrict to their transactions
////        if (userRole == nameof(Roles.User))
////        {
////            query = query.Where(t => t.CreatedBy == currentUserId.ToString());
////        }

////        // 🔍 Apply filters
////        if (!string.IsNullOrEmpty(request.Keyword))
////        {
////            query = query.Where(t =>
////                (t.TransactionType != null && t.TransactionType.Contains(request.Keyword)) ||
////                t.Id.ToString() == request.Keyword);
////        }

////        if (!string.IsNullOrEmpty(request.TransactionType))
////        {
////            query = query.Where(t => t.TransactionType == request.TransactionType);
////        }

////        if (request.StartDate.HasValue)
////        {
////            var startDateUtc = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
////            query = query.Where(t => t.TransactionDateTime >= startDateUtc);
////        }

////        if (request.EndDate.HasValue)
////        {
////            var endDate = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
////            var endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
////            query = query.Where(t => t.TransactionDateTime <= endDateUtc);
////        }

////        // 📦 Projection + Pagination
////        var paginatedTransactions = await query
////            .OrderByDescending(t => t.TransactionDateTime)
////            .Select(t => new TransactionDTO
////            {
////                Id = t.Id,
////                TransactionDateTime = t.TransactionDateTime,
////                TransactionAmount = t.TransactionAmount,
////                TransactionType = t.TransactionType ?? "N/A",
////                FromPayee = t.FromPayee ?? "N/A",
////                ToRecipient = t.ToRecipient ?? "N/A",
////                ContractId = t.ContractId,
////                ContractDetails = t.ContractId != null ? (
////                    from c in _context.ContractDetails
////                    where c.Id == t.ContractId
////                    select new ContractDetailsDTO
////                    {
////                        Id = c.Id,
////                        Role = c.Role,
////                        ContractTitle = c.ContractTitle,
////                        ServiceType = c.ServiceType,
////                        ServiceDescription = c.ServiceDescription,
////                        AdditionalNote = c.AdditionalNote,
////                        FeesPaidBy = c.FeesPaidBy,
////                        FeeAmount = c.FeeAmount,
////                        BuyerName = c.BuyerName,
////                        BuyerMobile = c.BuyerMobile,
////                        BuyerId = c.BuyerDetailsId.ToString(),
////                        SellerId = c.SellerDetailsId.ToString(),
////                        SellerName = c.SellerName,
////                        SellerMobile = c.SellerMobile,
////                        CreatedBy = c.CreatedBy,
////                        ContractDoc = c.ContractDoc,
////                        Status = c.Status,
////                        IsActive = c.IsActive,
////                        IsDeleted = c.IsDeleted,
////                        TaxAmount = c.TaxAmount,
////                        EscrowTax = c.EscrowTax,
////                        LastModifiedBy = c.LastModifiedBy,
////                        BuyerPayableAmount = c.BuyerPayableAmount,
////                        SellerPayableAmount = c.SellerPayableAmount,
////                        Created = c.Created,
////                        LastModified = c.LastModified,

////                        MileStones = _context.MileStones
////                            .Where(m => m.ContractId == c.Id)
////                            .Select(m => new MileStoneDTO
////                            {
////                                Id = m.Id,
////                                Name = m.Name,
////                                Amount = m.Amount,
////                                DueDate = m.DueDate,
////                                Status = m.Status
////                            }).ToList(),

////                        TeamMembers = new List<TeamDTO>(),
////                        InvitationDetails = null
////                    }).FirstOrDefault() : null
////            })
////            .PaginatedListAsync(pageNumber, pageSize);

////        return paginatedTransactions;
////    }
////}













////public async Task<PaginatedList<TransactionDTO>> Handle(SearchTransactionsQuery request, CancellationToken cancellationToken)
////{
////    var validator = new SearchTransactionsQueryValidator();
////    var validationResult = validator.Validate(request);
////    if (!validationResult.IsValid)
////    {
////        throw new ArgumentException(validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid input.");
////    }

////    int pageNumber = request.PageNumber ?? 1;
////    int pageSize = request.PageSize ?? 10;

////    var query = _context.Transactions.AsQueryable();

////    // 🔍 Filtering logic
////    if (!string.IsNullOrEmpty(request.Keyword))
////    {
////        query = query.Where(t => t.TransactionType != null && t.TransactionType.Contains(request.Keyword) || t.Id.ToString() == request.Keyword);
////    }

////    if (!string.IsNullOrEmpty(request.TransactionType))
////    {
////        query = query.Where(t => t.TransactionType == request.TransactionType);
////    }

////    if (request.StartDate.HasValue)
////    {
////        var startDateUtc = DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Utc);
////        query = query.Where(t => t.TransactionDateTime >= startDateUtc);
////    }

////    if (request.EndDate.HasValue)
////    {
////        var endDate = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
////        var endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
////        query = query.Where(t => t.TransactionDateTime <= endDateUtc);
////    }



////    //if (request.StartDate.HasValue)
////    //{
////    //    query = query.Where(t => t.TransactionDateTime.Date >= Convert.ToDateTime(request.StartDate.Value).Date);
////    //}

////    //if (request.EndDate.HasValue)
////    //{
////    //    query = query.Where(t => t.TransactionDateTime.Date <= request.EndDate.Value);
////    //}

////    // 📌 Apply Pagination
////    var paginatedTransactions = await query
////.OrderByDescending(t => t.TransactionDateTime)
////.Select(t => new TransactionDTO
////{
////    Id = t.Id,
////    TransactionDateTime = t.TransactionDateTime,
////    TransactionAmount = t.TransactionAmount,
////    TransactionType = t.TransactionType ?? "N/A",
////    FromPayee = t.FromPayee ?? "N/A",
////    ToRecipient = t.ToRecipient ?? "N/A",
////    ContractId = t.ContractId,
////    ContractDetails = t.ContractId != null ? (
////        from c in _context.ContractDetails
////        where c.Id == t.ContractId
////        select new ContractDetailsDTO
////        {
////            Id = c.Id,
////            Role = c.Role,
////            ContractTitle = c.ContractTitle,
////            ServiceType = c.ServiceType,
////            ServiceDescription = c.ServiceDescription,
////            AdditionalNote = c.AdditionalNote,
////            FeesPaidBy = c.FeesPaidBy,
////            FeeAmount = c.FeeAmount,
////            BuyerName = c.BuyerName,
////            BuyerMobile = c.BuyerMobile,
////            BuyerId = c.BuyerDetailsId.ToString(),
////            SellerId = c.SellerDetailsId.ToString(),
////            SellerName = c.SellerName,
////            SellerMobile = c.SellerMobile,
////            CreatedBy = c.CreatedBy,
////            ContractDoc = c.ContractDoc,
////            Status = c.Status,
////            IsActive = c.IsActive,
////            IsDeleted = c.IsDeleted,
////            TaxAmount = c.TaxAmount,
////            EscrowTax = c.EscrowTax,
////            LastModifiedBy = c.LastModifiedBy,
////            BuyerPayableAmount = c.BuyerPayableAmount,
////            SellerPayableAmount = c.SellerPayableAmount,
////            Created = c.Created,
////            LastModified = c.LastModified,

////            MileStones = _context.MileStones
////                .Where(m => m.ContractId == c.Id)
////                .Select(m => new MileStoneDTO
////                {
////                    Id = m.Id,
////                    Name = m.Name,
////                    Amount = m.Amount,
////                    DueDate = m.DueDate,
////                    Status = m.Status
////                }).ToList(),

////            // You can optionally fetch these similarly
////            TeamMembers = new List<TeamDTO>(),
////            InvitationDetails = null
////        }).FirstOrDefault() : null
////})
////.PaginatedListAsync(pageNumber, pageSize);


////    return paginatedTransactions;
////}

