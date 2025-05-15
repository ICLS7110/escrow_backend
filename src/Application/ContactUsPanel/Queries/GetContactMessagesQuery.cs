using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.ContactDTO;
using Escrow.Api.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Escrow.Api.Application.ContactUsPanel.Queries;
public record GetContactMessagesQuery : IRequest<PaginatedList<ContactUsDTO>>
{
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}
public class GetContactMessagesHandler : IRequestHandler<GetContactMessagesQuery, PaginatedList<ContactUsDTO>>
{
    private readonly IApplicationDbContext _context;

    public GetContactMessagesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ContactUsDTO>> Handle(GetContactMessagesQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        var query = _context.ContactUs.AsQueryable();

        return await query
            .Select(s => new ContactUsDTO
            {
                Name = s.Name,
                Number = s.Number,
                Email = s.Email,
                Title = s.Title,
                Message = s.Message
            })
            .OrderByDescending(x => x.Title)
            .PaginatedListAsync(pageNumber, pageSize);
    }
}
