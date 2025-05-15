using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.AssignPermissionDtos;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.PermissionManagers.Queries;
public class GetMenusQuery : IRequest<Result<List<MenuDto>>> { }

public class GetMenusQueryHandler : IRequestHandler<GetMenusQuery, Result<List<MenuDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetMenusQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MenuDto>>> Handle(GetMenusQuery request, CancellationToken cancellationToken)
    {
        var menus = await _context.Menus
            .AsNoTracking()
            .Select(menu => new MenuDto
            {
                Id = menu.Id,
                Name = menu.Name,
            })
            .ToListAsync(cancellationToken);

        if (menus == null || !menus.Any())
        {
            return Result<List<MenuDto>>.Failure(StatusCodes.Status404NotFound, "No menus found.");
        }

        return Result<List<MenuDto>>.Success(StatusCodes.Status200OK, "Menus fetched successfully", menus);
    }
}
