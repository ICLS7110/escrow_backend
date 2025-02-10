using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.UserPanel.Queries.GetUsers;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Infrastructure.Security;

namespace Escrow.Api.Application.BankDetails.Queries;

public record GetBankDetailsQuery : IRequest<PaginatedList<BankDetail>>
{
    public int? Id { get; init; }
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}

public class GetBankDetailsQueryHandler : IRequestHandler<GetBankDetailsQuery, PaginatedList<BankDetail>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    private readonly IAESService _AESService;

    public GetBankDetailsQueryHandler(IApplicationDbContext context, IMapper mapper,IJwtService jwtService, IAESService aESService)
    {
        _context = context;
        _mapper = mapper;
        _jwtService = jwtService;
        _AESService = aESService;
    }

    public async Task<PaginatedList<BankDetail>> Handle(GetBankDetailsQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        var query = _context.BankDetails.Where(x => x.RecordState == "Active").AsQueryable();

        if (request.Id.HasValue)
        {
            query = query.Where(x => x.Id == request.Id.Value);
        }

        return await query
            .Select(s => new BankDetail
            {
                Id = s.Id,
                UserDetail = s.UserDetail,
                UserDetailId = s.UserDetailId,
                AccountHolderName = s.AccountHolderName,
                IBANNumber = _AESService.Decrypt(s.IBANNumber),
                BICCode = s.BICCode
            })
            .OrderBy(x => x.AccountHolderName)
            .PaginatedListAsync(pageNumber, pageSize);
    }
}
