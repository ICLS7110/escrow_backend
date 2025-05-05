using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.SystemConfigurations;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.SystemConfigurations.Commands;
public class UpsertSystemConfigurationCommand : IRequest<Result<object>>
{
    public string Key { get; set; } = "monthly"!;
    public string Value { get; set; } = "10000";
}
public class UpsertSystemConfigurationCommandHandler : IRequestHandler<UpsertSystemConfigurationCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;

    public UpsertSystemConfigurationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object>> Handle(UpsertSystemConfigurationCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.SystemConfigurations
            .FirstOrDefaultAsync(x => x.Key == request.Key, cancellationToken);

        if (existing != null)
        {
            existing.Value = request.Value;
            existing.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(
                StatusCodes.Status200OK,
                "Configuration updated successfully",
                new
                {
                    id = existing.Id,
                    key = existing.Key,
                    value = existing.Value
                });
        }

        var config = new SystemConfiguration
        {
            Key = request.Key,
            Value = request.Value,
            Created = DateTime.UtcNow
        };

        await _context.SystemConfigurations.AddAsync(config, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<object>.Success(
            StatusCodes.Status201Created,
            "Configuration created successfully",
            new
            {
                id = config.Id,
                key = config.Key,
                value = config.Value
            });
    }
}
