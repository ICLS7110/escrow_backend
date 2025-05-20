using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.Commissions.Commands;
public class SetCommissionAsGlobalCommand : IRequest<Result<object>>
{
    public int Id { get; set; }
}

public class SetCommissionAsGlobalCommandHandler : IRequestHandler<SetCommissionAsGlobalCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SetCommissionAsGlobalCommandHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<object>> Handle(SetCommissionAsGlobalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            var commissionToSetGlobal = await _context.CommissionMasters.FindAsync(new object[] { request.Id }, cancellationToken);
            if (commissionToSetGlobal == null)
            {
                return Result<object>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("CommissionNotFound", language));
            }

            // Unset existing global commission(s)
            var existingGlobals = await _context.CommissionMasters
                .Where(x => x.AppliedGlobally && x.Id != request.Id)
                .ToListAsync(cancellationToken);

            foreach (var global in existingGlobals)
            {
                global.AppliedGlobally = false;
                global.LastModified = DateTime.UtcNow;
            }

            // Set new global
            commissionToSetGlobal.AppliedGlobally = true;
            commissionToSetGlobal.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("CommissionSetAsGlobal", language), commissionToSetGlobal);
        }
        catch (Exception ex)
        {
            return Result<object>.Failure(StatusCodes.Status500InternalServerError, $"Unexpected error: {ex.Message}");
        }
    }
}
