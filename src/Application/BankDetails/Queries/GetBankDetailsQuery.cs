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
    private readonly IRsaHelper _rsaHelper;

    public GetBankDetailsQueryHandler(IApplicationDbContext context, IMapper mapper,IJwtService jwtService, IRsaHelper rsaHelper)
    {
        _context = context;
        _mapper = mapper;
        _jwtService = jwtService;
        _rsaHelper = rsaHelper;
    }

    public async Task<PaginatedList<BankDetail>> Handle(GetBankDetailsQuery request, CancellationToken cancellationToken)
    {
        if (request.Id.HasValue)
        {
            return await _context.BankDetails
            .Where(x => x.Id == Convert.ToInt32(request.Id)).Select(s => new BankDetail()
            {
                Id = s.Id,
                UserDetail = s.UserDetail,
                UserDetailId=s.UserDetailId,
                AccountHolderName= s.AccountHolderName,
                IBANNumber=_rsaHelper.DecryptWithPrivateKey(s.IBANNumber),
                BICCode=_rsaHelper.DecryptWithPrivateKey(s.BICCode),
            })
            .OrderBy(x => x.AccountHolderName)
                //.ProjectTo<BankDetail>(_mapper.ConfigurationProvider)
                .PaginatedListAsync(request.PageNumber ?? 1, request.PageSize ?? 10);
        }
        else
        {
            return await _context.BankDetails.Select(s => new BankDetail()
            {
                Id = s.Id,
                UserDetail = s.UserDetail,
                UserDetailId = s.UserDetailId,
                AccountHolderName = s.AccountHolderName,
                IBANNumber = _rsaHelper.DecryptWithPrivateKey(s.IBANNumber),
                BICCode = _rsaHelper.DecryptWithPrivateKey(s.BICCode),
            })
                .OrderBy(x => x.AccountHolderName)
                //.ProjectTo<BankDetail>(_mapper.ConfigurationProvider)
                .PaginatedListAsync(request.PageNumber ?? 1, request.PageSize ?? 10);
        }
    }
}
