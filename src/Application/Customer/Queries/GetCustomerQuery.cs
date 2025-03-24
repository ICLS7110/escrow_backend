using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Domain.Entities.UserPanel;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Application.Common.Mappings;

namespace Escrow.Api.Application.Customers.Queries
{
    public record GetCustomerQuery : IRequest<PaginatedList<CustomerDto>>
    {
        public int? Id { get; init; } = 0;
        public string? Filter { get; init; }
        public int? PageNumber { get; init; } = 1;
        public int? PageSize { get; init; } = 10;
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
                .Where(u => u.DeletedBy == 0 || u.DeletedBy == null) // Fetch only active customers
                .AsQueryable();

            // 🔹 If ID is provided, fetch only that customer
            if (request.Id.HasValue && request.Id.Value > 0)
            {
                query = query.Where(x => x.Id == request.Id.Value);
            }

            // 🔹 If Name is provided, search customers whose names contain the input
            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                query = query.Where(x => x.FullName != null && x.FullName.Contains(request.Filter) || x.PhoneNumber != null && x.PhoneNumber.Contains(request.Filter) || x.EmailAddress != null && x.EmailAddress.Contains(request.Filter));
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
                })
                .OrderBy(x => x.FullName)
                .PaginatedListAsync(pageNumber, pageSize);
        }
    }
}
