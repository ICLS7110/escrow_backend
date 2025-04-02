using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;

namespace Escrow.Api.Application.Policies.Queries;
public record GetAllPagesQuery : IRequest<PaginatedList<PagesDTO>>
{
    public int? Id { get; init; } = 0;
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}

public class GetAllPagesQueryHandler : IRequestHandler<GetAllPagesQuery, PaginatedList<PagesDTO>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPagesQueryHandler(IApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PaginatedList<PagesDTO>> Handle(GetAllPagesQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        // ✅ Fix: Now using "Page" instead of "Pages"
        var query = _context.Pages
            .Where(x => x.IsDeleted == null || !x.IsDeleted.Value)
            .AsQueryable();

        if (request.Id.HasValue && request.Id.Value > 0)
        {
            query = query.Where(x => x.Id == request.Id.Value);
        }

        return await query
            .Select(p => new PagesDTO
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Content = p.Content,
                IsActive = p.IsActive,
                DeletedBy = p.DeletedBy
            })
            .OrderBy(x => x.Title)
            .PaginatedListAsync(pageNumber, pageSize);
    }
}
