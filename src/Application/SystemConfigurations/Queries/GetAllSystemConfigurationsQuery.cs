using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.SystemConfiguration;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.SystemConfigurations.Queries;
public class GetAllSystemConfigurationsQuery : IRequest<Result<List<SystemConfigurationDTO>>>
{
    public string? Key { get; set; } // Optional key filter
}
public class GetAllSystemConfigurationsQueryHandler : IRequestHandler<GetAllSystemConfigurationsQuery, Result<List<SystemConfigurationDTO>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllSystemConfigurationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SystemConfigurationDTO>>> Handle(GetAllSystemConfigurationsQuery request, CancellationToken cancellationToken)
    {
        var configurations = await _context.SystemConfigurations
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if(request.Key != null)
        {
            configurations = configurations.Where(c => c.Key == request.Key).ToList();
        }

        var mappedList = configurations.Select(config => new SystemConfigurationDTO
        {
            Key = config.Key,
            Value = config.Value,
        }).ToList();

        return Result<List<SystemConfigurationDTO>>.Success(StatusCodes.Status200OK, "Success", mappedList);
    }
}
