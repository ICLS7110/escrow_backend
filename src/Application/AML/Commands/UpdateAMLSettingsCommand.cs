using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.AML.Commands;

public class UpdateAMLSettingsCommand : IRequest<Result<object>>
{
    public decimal HighAmountLimit { get; set; }
    public string FrequencyThreshold { get; set; } = string.Empty;
    public decimal BenchmarkLimit { get; set; }
}

// Commenting out validation for now
// public class UpdateAMLSettingsValidator : AbstractValidator<UpdateAMLSettingsCommand>
// {
//     public UpdateAMLSettingsValidator()
//     {
//         RuleFor(x => x.HighAmountLimit)
//             .GreaterThan(0).WithMessage("High amount limit must be greater than zero.");

//         RuleFor(x => x.FrequencyThreshold)
//             .NotEmpty().WithMessage("Frequency threshold is required.")
//             .Must(x => new[] { "Hourly", "Daily", "Weekly", "Monthly" }.Contains(x))
//             .WithMessage("Invalid frequency threshold. Allowed values: Hourly, Daily, Weekly, Monthly.");

//         RuleFor(x => x.BenchmarkLimit)
//             .GreaterThan(0).WithMessage("Benchmark limit must be greater than zero.");
//     }
// }

public class UpdateAMLSettingsCommandHandler : IRequestHandler<UpdateAMLSettingsCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;

    public UpdateAMLSettingsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object>> Handle(UpdateAMLSettingsCommand request, CancellationToken cancellationToken)
    {
        // 🔹 Validation commented out for now
        // var validator = new UpdateAMLSettingsValidator();
        // var validationResult = await validator.ValidateAsync(request, cancellationToken);

        // if (!validationResult.IsValid)
        // {
        //     string errorMessages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
        //     return Result<object>.Failure(StatusCodes.Status400BadRequest, errorMessages);
        // }

        var settings = await _context.AMLSettings.FirstOrDefaultAsync(cancellationToken);

        if (settings == null)
        {
            return Result<object>.Failure(StatusCodes.Status404NotFound, "AML settings not found.");
        }

        settings.HighAmountLimit = request.HighAmountLimit;
        settings.FrequencyThreshold = request.FrequencyThreshold;
        settings.BenchmarkLimit = request.BenchmarkLimit;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<object>.Success(StatusCodes.Status200OK, "AML settings updated successfully.");
    }
}
