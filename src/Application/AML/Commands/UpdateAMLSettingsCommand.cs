using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.AML.Commands;

public class UpdateAMLSettingsCommand : IRequest<Result<object>>
{
    public decimal HighAmountLimit { get; set; }
    public string FrequencyThreshold { get; set; } = string.Empty;
    public decimal BenchmarkLimit { get; set; }
}

// Optional: Enable when needed
// public class UpdateAMLSettingsValidator : AbstractValidator<UpdateAMLSettingsCommand>
// {
//     public UpdateAMLSettingsValidator()
//     {
//         RuleFor(x => x.HighAmountLimit).GreaterThan(0);
//         RuleFor(x => x.FrequencyThreshold).NotEmpty();
//         RuleFor(x => x.BenchmarkLimit).GreaterThan(0);
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
        var newSettings = new Domain.Entities.AMLPanel.AMLSettings
        {
            HighAmountLimit = request.HighAmountLimit,
            FrequencyThreshold = request.FrequencyThreshold,
            BenchmarkLimit = request.BenchmarkLimit,
        };

        await _context.AMLSettings.AddAsync(newSettings, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object>.Success(
            StatusCodes.Status200OK,
            "AML settings created successfully.",
            new
            {
                newSettings.Id,
                newSettings.HighAmountLimit,
                newSettings.FrequencyThreshold,
                newSettings.BenchmarkLimit
            });
    }
}
