
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.Commissions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace Escrow.Api.Application.Commissions.Commands
{
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
        private readonly IStringLocalizer<LocalizationKeys> _localizer;
        public UpsertCommissionRateCommandValidator(IStringLocalizer<LocalizationKeys> localizer)
        {
            _localizer = localizer;

            RuleFor(x => x.CommissionRate)
                .GreaterThan(0)
                .WithMessage(_localizer[LocalizationKeys.CommissionRateMustBeGreaterThanZero]);

            RuleFor(x => x.TransactionType)
                .NotEmpty()
                .WithMessage(_localizer[LocalizationKeys.TransactionTypeIsRequired]);

            RuleFor(x => x.TaxRate)
                .GreaterThanOrEqualTo(0)
                .WithMessage(_localizer[LocalizationKeys.TaxRateCannotBeNegative]);
        }

        //public UpsertCommissionRateCommandValidator()
        //{
        //    RuleFor(x => x.CommissionRate).GreaterThan(0).WithMessage(AppMessages.CommissionRateMustBeGreaterThanZero);
        //    RuleFor(x => x.TransactionType).NotEmpty().WithMessage(AppMessages.TransactionTypeIsRequired);
        //    RuleFor(x => x.TaxRate).GreaterThanOrEqualTo(0).WithMessage(AppMessages.TaxRateCannotBeNegative);
        //}

        public class LocalizationKeys
        {
            public static string CommissionRateMustBeGreaterThanZero => "CommissionRateMustBeGreaterThanZero";
            public static string TransactionTypeIsRequired => "TransactionTypeIsRequired";
            public static string TaxRateCannotBeNegative => "TaxRateCannotBeNegative";
        }

    }

    public class UpsertCommissionRateCommandHandler : IRequestHandler<UpsertCommissionRateCommand, Result<object>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpsertCommissionRateCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<Result<object>> Handle(UpsertCommissionRateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

                // Check if a commission already exists for the same TransactionType
                var existingTransactionTypeCommission = await _context.CommissionMasters
                    .Where(x => x.TransactionType == request.TransactionType && x.Id != request.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingTransactionTypeCommission != null)
                {
                    return Result<object>.Failure(StatusCodes.Status400BadRequest,
                        AppMessages.Get("TransactionTypeCommissionExists", language)); // Add this to LanguageMessageHelper
                }

                // Ensure only one global commission can exist
                if (request.AppliedGlobally)
                {
                    var existingGlobalCommission = await _context.CommissionMasters
                        .Where(x => x.AppliedGlobally && x.Id != request.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (existingGlobalCommission != null)
                    {
                        return Result<object>.Failure(StatusCodes.Status400BadRequest,
                            AppMessages.Get("GlobalCommissionExists", language));
                    }
                }

                var commission = await _context.CommissionMasters.FindAsync(new object[] { request.Id }, cancellationToken);

                if (commission == null)
                {
                    // Insert logic
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

                    return Result<object>.Success(StatusCodes.Status201Created, AppMessages.Get("CommissionCreated", language), newCommission);
                }
                else
                {
                    // Check if we are trying to unset the only global commission
                    if (commission.AppliedGlobally && !request.AppliedGlobally)
                    {
                        var otherGlobalExists = await _context.CommissionMasters
                            .AnyAsync(x => x.AppliedGlobally && x.Id != request.Id, cancellationToken);

                        if (!otherGlobalExists)
                        {
                            return Result<object>.Failure(StatusCodes.Status400BadRequest,
                                AppMessages.Get("AtLeastOneGlobalCommissionRequired", language));
                        }
                    }

                    // Update logic
                    commission.CommissionRate = request.CommissionRate;
                    commission.AppliedGlobally = request.AppliedGlobally;
                    commission.TransactionType = request.TransactionType;
                    commission.TaxRate = request.TaxRate;
                    commission.MinAmount = request.MinAmount?.ToString();
                    commission.LastModified = DateTime.UtcNow;

                    await _context.SaveChangesAsync(cancellationToken);

                    return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("CommissionUpdated", language), commission);
                }
            }
            catch (Exception ex)
            {
                return Result<object>.Failure(StatusCodes.Status500InternalServerError, $"Unexpected error: {ex.Message}");
            }
        }



















        //public async Task<Result<object>> Handle(UpsertCommissionRateCommand request, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        //        if (request.AppliedGlobally)
        //        {
        //            var existingGlobalCommission = await _context.CommissionMasters
        //                .Where(x => x.AppliedGlobally && x.Id != request.Id)
        //                .FirstOrDefaultAsync(cancellationToken);

        //            if (existingGlobalCommission != null)
        //            {
        //                return Result<object>.Failure(StatusCodes.Status400BadRequest,
        //                    AppMessages.Get("GlobalCommissionExists", language));
        //            }
        //        }

        //        var commission = await _context.CommissionMasters.FindAsync(new object[] { request.Id }, cancellationToken);

        //        if (commission == null)
        //        {
        //            // Insert logic
        //            var newCommission = new CommissionMaster
        //            {
        //                Id = request.Id,
        //                CommissionRate = request.CommissionRate,
        //                AppliedGlobally = request.AppliedGlobally,
        //                TransactionType = request.TransactionType,
        //                TaxRate = request.TaxRate,
        //                MinAmount = request.MinAmount?.ToString(),
        //                Created = DateTime.UtcNow,
        //                LastModified = DateTime.UtcNow
        //            };

        //            _context.CommissionMasters.Add(newCommission);
        //            await _context.SaveChangesAsync(cancellationToken);

        //            return Result<object>.Success(StatusCodes.Status201Created, AppMessages.Get("CommissionCreated", language), newCommission);
        //        }
        //        else
        //        {
        //            // Check if we are trying to unset the only global commission
        //            if (commission.AppliedGlobally && !request.AppliedGlobally)
        //            {
        //                var otherGlobalExists = await _context.CommissionMasters
        //                    .AnyAsync(x => x.AppliedGlobally && x.Id != request.Id, cancellationToken);

        //                if (!otherGlobalExists)
        //                {
        //                    return Result<object>.Failure(StatusCodes.Status400BadRequest,
        //                        AppMessages.Get("AtLeastOneGlobalCommissionRequired", language)); // Define this key
        //                }
        //            }

        //            // Update logic
        //            commission.CommissionRate = request.CommissionRate;
        //            commission.AppliedGlobally = request.AppliedGlobally;
        //            commission.TransactionType = request.TransactionType;
        //            commission.TaxRate = request.TaxRate;
        //            commission.MinAmount = request.MinAmount?.ToString();
        //            commission.LastModified = DateTime.UtcNow;

        //            await _context.SaveChangesAsync(cancellationToken);

        //            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("CommissionUpdated", language), commission);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Result<object>.Failure(StatusCodes.Status500InternalServerError, $"Unexpected error: {ex.Message}");
        //    }
        //}















        //public async Task<Result<object>> Handle(UpsertCommissionRateCommand request, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        // Get current language from the HttpContext
        //        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

        //        if (request.AppliedGlobally)
        //        {
        //            var existingGlobalCommission = await _context.CommissionMasters
        //                .Where(x => x.AppliedGlobally && x.Id != request.Id)
        //                .FirstOrDefaultAsync(cancellationToken);

        //            if (existingGlobalCommission != null)
        //            {
        //                return Result<object>.Failure(StatusCodes.Status400BadRequest,
        //                    AppMessages.Get("GlobalCommissionExists", language)); // Define this key in your LanguageMessageHelper
        //            }
        //        }


        //        var commission = await _context.CommissionMasters.FindAsync(new object[] { request.Id }, cancellationToken);

        //        if (commission == null)
        //        {
        //            // Insert logic (Upsert)
        //            var newCommission = new CommissionMaster
        //            {
        //                Id = request.Id,
        //                CommissionRate = request.CommissionRate,
        //                AppliedGlobally = request.AppliedGlobally,
        //                TransactionType = request.TransactionType,
        //                TaxRate = request.TaxRate,
        //                MinAmount = request.MinAmount?.ToString(),
        //                Created = DateTime.UtcNow,
        //                LastModified = DateTime.UtcNow
        //            };

        //            _context.CommissionMasters.Add(newCommission);
        //            await _context.SaveChangesAsync(cancellationToken);

        //            return Result<object>.Success(StatusCodes.Status201Created, AppMessages.Get("CommissionCreated", language), newCommission);
        //        }
        //        else
        //        {
        //            // Update logic
        //            commission.CommissionRate = request.CommissionRate;
        //            commission.AppliedGlobally = request.AppliedGlobally;
        //            commission.TransactionType = request.TransactionType;
        //            commission.TaxRate = request.TaxRate;
        //            commission.MinAmount = request.MinAmount?.ToString();
        //            commission.LastModified = DateTime.UtcNow;

        //            await _context.SaveChangesAsync(cancellationToken);

        //            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("CommissionUpdated", language), commission);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exception
        //        return Result<object>.Failure(StatusCodes.Status500InternalServerError, $"Unexpected error: {ex.Message}");
        //    }
        //}
    }
}










































//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Entities.Commissions;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.Commissions.Commands;

//public record UpsertCommissionRateCommand(
//    int Id,
//    decimal CommissionRate,
//    bool AppliedGlobally,
//    string TransactionType,
//    decimal? MinAmount,
//    decimal TaxRate
//) : IRequest<Result<object>>;

//public class UpsertCommissionRateCommandValidator : AbstractValidator<UpsertCommissionRateCommand>
//{
//    public UpsertCommissionRateCommandValidator()
//    {
//        RuleFor(x => x.CommissionRate).GreaterThan(0).WithMessage(AppMessages.Commissionratemustbegreaterthanzero);
//        RuleFor(x => x.TransactionType).NotEmpty().WithMessage(AppMessages.Transactiontypeisrequired);
//        RuleFor(x => x.TaxRate).GreaterThanOrEqualTo(0).WithMessage(AppMessages.Taxratecannotbenegative);
//    }
//}

//public class UpsertCommissionRateCommandHandler : IRequestHandler<UpsertCommissionRateCommand, Result<object>>
//{
//    private readonly IApplicationDbContext _context;

//    public UpsertCommissionRateCommandHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<object>> Handle(UpsertCommissionRateCommand request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            var commission = await _context.CommissionMasters.FindAsync(new object[] { request.Id }, cancellationToken);

//            if (commission == null)
//            {
//                // INSERT logic (Upsert)
//                var newCommission = new CommissionMaster
//                {
//                    Id = request.Id,
//                    CommissionRate = request.CommissionRate,
//                    AppliedGlobally = request.AppliedGlobally,
//                    TransactionType = request.TransactionType,
//                    TaxRate = request.TaxRate,
//                    MinAmount = request.MinAmount?.ToString(),
//                    Created = DateTime.UtcNow,
//                    LastModified = DateTime.UtcNow
//                };

//                _context.CommissionMasters.Add(newCommission);
//                await _context.SaveChangesAsync(cancellationToken);

//                return Result<object>.Success(StatusCodes.Status201Created, AppMessages.Commissioncreated, newCommission);
//            }
//            else
//            {
//                // UPDATE logic
//                commission.CommissionRate = request.CommissionRate;
//                commission.AppliedGlobally = request.AppliedGlobally;
//                commission.TransactionType = request.TransactionType;
//                commission.TaxRate = request.TaxRate;
//                commission.MinAmount = request.MinAmount?.ToString();
//                commission.LastModified = DateTime.UtcNow;

//                await _context.SaveChangesAsync(cancellationToken);

//                return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Commissionupdated, commission);
//            }
//        }
//        catch (Exception ex)
//        {
//            return Result<object>.Failure(StatusCodes.Status500InternalServerError, $"Unexpected error: {ex.Message}");
//        }
//    }
//}
