using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Domain.Entities.UserPanel;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.Common.Mappings;
using Escrow.Api.Domain.Enums;

namespace Escrow.Api.Application.Customers.Queries
{
    public record GetCustomerQuery : IRequest<PaginatedList<CustomerDto>>
    {
        public int? Id { get; init; } = 0;
        public string? Filter { get; init; }
        public int? PageNumber { get; init; } = 1;
        public int? PageSize { get; init; } = 10;
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
    }

    public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, PaginatedList<CustomerDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCustomerQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<PaginatedList<CustomerDto>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
        {
            int pageNumber = request.PageNumber ?? 1;
            int pageSize = request.PageSize ?? 10;

            var query = _context.UserDetails
                .Where(u => !u.IsDeleted && u.Role == nameof(Roles.User)) // Fetch only active customers
                .AsQueryable();

            // 🔹 If ID is provided, fetch only that customer
            if (request.Id.HasValue && request.Id.Value > 0)
            {
                query = query.Where(x => x.Id == request.Id.Value);
            }

            // 🔹 Filter by name/email/phone
            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                query = query.Where(x =>
                    (x.FullName != null && x.FullName.Contains(request.Filter)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.Contains(request.Filter)) ||
                    (x.EmailAddress != null && x.EmailAddress.Contains(request.Filter)));
            }

            // 🔹 Filter by date only (ignoring time)
            if (request.StartDate.HasValue)
            {
                var startDate = request.StartDate.Value.Date;
                query = query.Where(x => x.Created.Date >= startDate);
            }

            if (request.EndDate.HasValue)
            {
                var endDate = request.EndDate.Value.Date;
                query = query.Where(x => x.Created.Date <= endDate);
            }

            return await query
                .Select(s => new CustomerDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    EmailAddress = s.EmailAddress,
                    PhoneNumber = s.PhoneNumber,
                    Gender = s.Gender,
                    DateOfBirth = s.DateOfBirth.ToString(),
                    BusinessManagerName = s.BusinessManagerName,
                    BusinessEmail = s.BusinessEmail,
                    VatId = s.VatId,
                    LoginMethod = s.LoginMethod,
                    Created = s.Created.ToString(),
                    CreatedBy = s.CreatedBy,
                    LastModified = s.LastModified.ToString(),
                    LastModifiedBy = s.LastModifiedBy,
                    BusinessProof = s.BusinessProof,
                    ProfilePicture = s.ProfilePicture,
                    IsProfileCompleted = s.IsProfileCompleted,
                    CompanyEmail = s.CompanyEmail,
                    IsActive = s.IsActive,

                    TotalFeesAmount = _context.ContractDetails
                        .Where(c => (c.BuyerDetailsId == s.Id || c.SellerDetailsId == s.Id || c.CreatedBy == s.Id.ToString()) && c.IsDeleted == false)
                        .Sum(c => (decimal?)c.FeeAmount) ?? 0
                })
                .OrderByDescending(x => x.Created)
                .PaginatedListAsync(pageNumber, pageSize);
        }

        //public async Task<PaginatedList<CustomerDto>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
        //{
        //    int pageNumber = request.PageNumber ?? 1;
        //    int pageSize = request.PageSize ?? 10;


        //    var query = _context.UserDetails
        //        .Where(u => !u.IsDeleted && u.Role == nameof(Roles.User)) // Fetch only active customers
        //        .AsQueryable();

        //    // 🔹 If ID is provided, fetch only that customer
        //    if (request.Id.HasValue && request.Id.Value > 0)
        //    {
        //        query = query.Where(x => x.Id == request.Id.Value);
        //    }

        //    // 🔹 If Name is provided, search customers whose names contain the input
        //    if (!string.IsNullOrWhiteSpace(request.Filter))
        //    {
        //        query = query.Where(x => x.FullName != null && x.FullName.Contains(request.Filter) || x.PhoneNumber != null && x.PhoneNumber.Contains(request.Filter) || x.EmailAddress != null && x.EmailAddress.Contains(request.Filter));
        //    }

        //    if (request.StartDate.HasValue)
        //    {
        //        query = query.Where(x => x.Created >= request.StartDate.Value);
        //    }

        //    if (request.EndDate.HasValue)
        //    {
        //        query = query.Where(x => x.Created <= request.EndDate.Value);
        //    }
        //    return await query
        //        .Select(s => new CustomerDto
        //        {
        //            Id = s.Id,
        //            FullName = s.FullName,
        //            EmailAddress = s.EmailAddress,
        //            PhoneNumber = s.PhoneNumber,
        //            Gender = s.Gender,
        //            DateOfBirth = s.DateOfBirth.ToString(),
        //            BusinessManagerName = s.BusinessManagerName,
        //            BusinessEmail = s.BusinessEmail,
        //            VatId = s.VatId,
        //            LoginMethod = s.LoginMethod,
        //            Created = s.Created.ToString(),
        //            CreatedBy = s.CreatedBy,
        //            LastModified = s.LastModified.ToString(),
        //            LastModifiedBy = s.LastModifiedBy,
        //            BusinessProof = s.BusinessProof,
        //            ProfilePicture = s.ProfilePicture,
        //            IsProfileCompleted = s.IsProfileCompleted,
        //            CompanyEmail = s.CompanyEmail,
        //            IsActive = s.IsActive,

        //            TotalFeesAmount = _context.ContractDetails
        //        .Where(c => (c.Id == s.Id || c.BuyerDetailsId==s.Id || c.SellerDetailsId==s.Id || c.CreatedBy == s.Id.ToString()) && c.IsDeleted == false)
        //        .Sum(c => (decimal?)c.FeeAmount) ?? 0
        //        })
        //        .OrderByDescending(x => x.Created)
        //        .PaginatedListAsync(pageNumber, pageSize);
        //}
    }
}
