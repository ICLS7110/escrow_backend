using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Domain.Entities.UserPanel;
using PhoneNumbers;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Application.Common.Helpers;

namespace Escrow.Api.Application.UserPanel.Queries.GetUsers;

public record GetUserDetailsQuery : IRequest<PaginatedList<UserDetailDto>>
{
    public int? Id { get; init; }
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 10;
}

public class GetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, PaginatedList<UserDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private static readonly PhoneNumberUtil _phoneUtil = PhoneNumberUtil.GetInstance(); // ✅ Make static

    public GetUserDetailsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<UserDetailDto>> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.PageNumber ?? 1;
        int pageSize = request.PageSize ?? 10;

        var query = _context.UserDetails
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (request.Id.HasValue)
        {
            query = query.Where(x => x.Id == request.Id.Value);
        }

        return await query
            .OrderBy(x => x.FullName)
            .Select(s => new UserDetailDto
            {
                Id = s.Id,
                FullName = s.FullName,
                EmailAddress = s.EmailAddress,
                PhoneNumber = PhoneNumberHelper.ExtractPhoneNumberWithoutCountryCode(s.PhoneNumber), // ✅ Call Helper
                CountryCode = PhoneNumberHelper.ExtractCountryCode(s.PhoneNumber), // ✅ Call Helper
                Gender = s.Gender,
                DateOfBirth = s.DateOfBirth,
                BusinessManagerName = s.BusinessManagerName,
                BusinessEmail = s.BusinessEmail,
                VatId = s.VatId,
                LoginMethod = s.LoginMethod,
                ProfilePicture = s.ProfilePicture,
                AccountType = s.AccountType,
                IsProfileCompleted = s.IsProfileCompleted,
                CompanyEmail = s.CompanyEmail
            })
            .PaginatedListAsync(pageNumber, pageSize);
    }
}
