using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.AML;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.AML.Queries;

public class GetAMLSettingsQuery : IRequest<Result<AMLSettingsDto>>
{
}

public class GetAMLSettingsQueryHandler : IRequestHandler<GetAMLSettingsQuery, Result<AMLSettingsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAMLSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AMLSettingsDto>> Handle(GetAMLSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _context.AMLSettings
            .OrderByDescending(x => x.Created) // Sort by newest first
            .FirstOrDefaultAsync(cancellationToken);

        if (settings == null)
        {
            return Result<AMLSettingsDto>.Failure(StatusCodes.Status404NotFound, "AML settings not found.");
        }

        var amlSettingsDto = new AMLSettingsDto
        {
            AMLSettingsId = settings.Id,
            HighAmountLimit = settings.HighAmountLimit,
            FrequencyThreshold = settings.FrequencyThreshold,
            BenchmarkLimit = settings.BenchmarkLimit
        };

        return Result<AMLSettingsDto>.Success(StatusCodes.Status200OK, "AML settings retrieved successfully.", amlSettingsDto);
    }
}










































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models.AML;
//using Escrow.Api.Application.DTOs;
//using Microsoft.AspNetCore.Http;

//namespace Escrow.Api.Application.AML.Queries;
//public class GetAMLSettingsQuery : IRequest<Result<AMLSettingsDto>>
//{
//}

//public class GetAMLSettingsQueryHandler : IRequestHandler<GetAMLSettingsQuery, Result<AMLSettingsDto>>
//{
//    private readonly IApplicationDbContext _context;

//    public GetAMLSettingsQueryHandler(IApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<AMLSettingsDto>> Handle(GetAMLSettingsQuery request, CancellationToken cancellationToken)
//    {
//        var settings = await _context.AMLSettings.FirstOrDefaultAsync(cancellationToken);

//        if (settings == null)
//        {
//            return Result<AMLSettingsDto>.Failure(StatusCodes.Status404NotFound, "AML settings not found.");
//        }

//        var amlSettingsDto = new AMLSettingsDto
//        {
//            HighAmountLimit = settings.HighAmountLimit,
//            FrequencyThreshold = settings.FrequencyThreshold,
//            BenchmarkLimit = settings.BenchmarkLimit
//        };

//        return Result<AMLSettingsDto>.Success(StatusCodes.Status200OK, "AML settings retrieved successfully.", amlSettingsDto);
//    }
//}
