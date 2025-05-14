
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace Escrow.Api.Application.AML.Commands
{
    public class AMLTransactionVerificationCommand : IRequest<Result<object>>
    {
        public int TransactionId { get; set; }
        public string AdminAction { get; set; } = string.Empty;
    }

    // Validation commented out for now
    // public class AMLTransactionVerificationValidator : AbstractValidator<AMLTransactionVerificationCommand>
    // {
    //     public AMLTransactionVerificationValidator()
    //     {
    //         RuleFor(x => x.TransactionId)
    //             .GreaterThan(0).WithMessage("Transaction ID must be greater than zero.");
    //
    //         RuleFor(x => x.AdminAction)
    //             .NotEmpty().WithMessage("Admin action is required.")
    //             .Must(x => new[] { "approve", "hold", "flag_for_investigation" }.Contains(x))
    //             .WithMessage("Invalid admin action. Allowed values: approve, hold, flag_for_investigation.");
    //     }
    // }

    public class AMLTransactionVerificationCommandHandler : IRequestHandler<AMLTransactionVerificationCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AMLTransactionVerificationCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<object>> Handle(AMLTransactionVerificationCommand request, CancellationToken cancellationToken)
        {
            // Get the current language from HttpContext
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            var transaction = await _context.AMLFlaggedTransactions
                .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

            if (transaction == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("TransactionNotFound", language));
            }

            if (!Enum.TryParse<AMLTransactionStatus>(request.AdminAction, true, out var action))
            {
                return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("InvalidAction", language));
            }

            transaction.Status = action.ToString();
            transaction.IsVerified = true;
            transaction.VerifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("TransactionVerified", language));
        }
    }
}






































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Enums;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.AML.Commands;
//public class AMLTransactionVerificationCommand : IRequest<Result<object>>
//{
//    public int TransactionId { get; set; }
//    public string AdminAction { get; set; } = string.Empty;
//}

//// Validation commented out for now
//// public class AMLTransactionVerificationValidator : AbstractValidator<AMLTransactionVerificationCommand>
//// {
////     public AMLTransactionVerificationValidator()
////     {
////         RuleFor(x => x.TransactionId)
////             .GreaterThan(0).WithMessage("Transaction ID must be greater than zero.");
////
////         RuleFor(x => x.AdminAction)
////             .NotEmpty().WithMessage("Admin action is required.")
////             .Must(x => new[] { "approve", "hold", "flag_for_investigation" }.Contains(x))
////             .WithMessage("Invalid admin action. Allowed values: approve, hold, flag_for_investigation.");
////     }
//// }

//public class AMLTransactionVerificationCommandHandler : IRequestHandler<AMLTransactionVerificationCommand, Result<object>>
//{
//    private readonly IApplicationDbContext _context;

//    public AMLTransactionVerificationCommandHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<object>> Handle(AMLTransactionVerificationCommand request, CancellationToken cancellationToken)
//    {
//        // 🔹 Validation commented out for now
//        // var validator = new AMLTransactionVerificationValidator();
//        // var validationResult = await validator.ValidateAsync(request, cancellationToken);
//        // if (!validationResult.IsValid)
//        // {
//        //     string errorMessages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
//        //     return Result<object>.Failure(StatusCodes.Status400BadRequest, errorMessages);
//        // }

//        var transaction = await _context.AMLFlaggedTransactions
//            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

//        if (transaction == null)
//        {
//            return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Transactionnotfound);
//        }

//        if (!Enum.TryParse<AMLTransactionStatus>(request.AdminAction, true, out var action))
//        {
//            return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.InvalidAction);
//        }

//        transaction.Status = action.ToString();
//        transaction.IsVerified = true;
//        transaction.VerifiedAt = DateTime.UtcNow;

//        await _context.SaveChangesAsync(cancellationToken);

//        return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Transactionverified);
//    }
//}
