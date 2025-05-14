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
using Escrow.Api.Domain.Enums;

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
        if (!request.Id.HasValue)
        {
            throw new ArgumentException("User ID must be provided.");
        }

       

        // Fetch user details based on provided user ID
        var user = await _context.UserDetails
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Id == request.Id.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        // Calculate confidence rate for the user
        var confidenceRate = await ConfidenceRateHelper.CalculateConfidenceRate(user.Id, _context);

        // Create the UserDetailDto with the confidence rate
        var userDetailDto = new UserDetailDto
        {
            Id = user.Id,
            FullName = user.FullName,
            EmailAddress = user.EmailAddress,
            PhoneNumber = PhoneNumberHelper.ExtractPhoneNumberWithoutCountryCode(user.PhoneNumber), // Call Helper
            CountryCode = PhoneNumberHelper.ExtractCountryCode(user.PhoneNumber), // Call Helper
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            BusinessManagerName = user.BusinessManagerName,
            BusinessEmail = user.BusinessEmail,
            VatId = user.VatId,
            LoginMethod = user.LoginMethod,
            BusinessProof = user.BusinessProof,
            ProfilePicture = user.ProfilePicture,
            AccountType = user.AccountType,
            IsProfileCompleted = user.IsProfileCompleted,
            DeviceToken = user.DeviceToken,
            CompanyEmail = user.CompanyEmail,
            IsNotified = user.IsNotified,
            SocialId = user.SocialId,
            ProfileCompletionPercentage = ProfileCompletionHelper.Calculate(user), // Profile Completion Helper
            ConfidenceRate = confidenceRate // Confidence Rate
        };

        // Wrapping the user detail DTO in a PaginatedList for consistency
        var paginatedList = new PaginatedList<UserDetailDto>(new List<UserDetailDto> { userDetailDto }, 1, 1, 1);

        return paginatedList;
    }


    //public async Task<PaginatedList<UserDetailDto>> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    //{
    //    int pageNumber = request.PageNumber ?? 1;
    //    int pageSize = request.PageSize ?? 10;

    //    var query = _context.UserDetails
    //        .AsNoTracking()
    //        .Where(x => !x.IsDeleted);

    //    if (request.Id.HasValue)
    //    {
    //        query = query.Where(x => x.Id == request.Id.Value);
    //    }

    //    return await query
    //        .OrderBy(x => x.FullName)
    //        .Select(s => new UserDetailDto
    //        {
    //            Id = s.Id,
    //            FullName = s.FullName,
    //            EmailAddress = s.EmailAddress,
    //            PhoneNumber = PhoneNumberHelper.ExtractPhoneNumberWithoutCountryCode(s.PhoneNumber), // ✅ Call Helper
    //            CountryCode = PhoneNumberHelper.ExtractCountryCode(s.PhoneNumber), // ✅ Call Helper
    //            Gender = s.Gender,
    //            DateOfBirth = s.DateOfBirth,
    //            BusinessManagerName = s.BusinessManagerName,
    //            BusinessEmail = s.BusinessEmail,
    //            VatId = s.VatId,
    //            LoginMethod = s.LoginMethod,
    //            BusinessProof = s.BusinessProof,
    //            ProfilePicture = s.ProfilePicture,
    //            AccountType = s.AccountType,
    //            IsProfileCompleted = s.IsProfileCompleted,
    //            DeviceToken = s.DeviceToken,
    //            CompanyEmail = s.CompanyEmail,
    //            IsNotified = s.IsNotified,
    //            SocialId = s.SocialId,
    //            ProfileCompletionPercentage = ProfileCompletionHelper.Calculate(s)
    //        })
    //        .PaginatedListAsync(pageNumber, pageSize);
    //}
}
