using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Domain.Entities.ContactUs;

namespace Escrow.Api.Application.ContactUsCommands.Queries;
public record GetContactUsDetailsQuery : IRequest<PaginatedList<ContactUs>>
{
    public int? Id { get; init; }
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}

public class GetContactUsDetailsQueryHandler : IRequestHandler<GetContactUsDetailsQuery, PaginatedList<ContactUs>>
{
    private readonly IApplicationDbContext _applicationDbContext;
    public GetContactUsDetailsQueryHandler(IApplicationDbContext applicationDbContext)
    {
            _applicationDbContext = applicationDbContext;
    }

    public async Task<PaginatedList<ContactUs>> Handle(GetContactUsDetailsQuery request,CancellationToken cancellationToken)
    {
        var query = _applicationDbContext.ContactUs.AsQueryable();

        if (request.Id.HasValue)
        {
            query = query.Where(x => x.Id == request.Id.Value);
        }

        return await query.OrderBy(o => o.Created)
                          .PaginatedListAsync(request.PageNumber ?? 1, request.PageSize ?? 10);

    }
}
