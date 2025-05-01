using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Helpers;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.ContractPanel.ContractQueries
{
    public record GetContractsQuery : IRequest<PaginatedList<ContractDetailsDTO>>
    {
        public int? UserId { get; init; }
        public ContractStatus? Status { get; init; }
        public string? SearchKeyword { get; init; }
        public bool? IsActive { get; init; }
        public int? PriceFilter { get; init; }
        public bool? IsMilestone { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }

    public class GetContractsQueryHandler : IRequestHandler<GetContractsQuery, PaginatedList<ContractDetailsDTO>>
    {
        private readonly IApplicationDbContext _context;

        public GetContractsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<ContractDetailsDTO>> Handle(GetContractsQuery request, CancellationToken cancellationToken)
        {
            var activeStatuses = new List<string> { nameof(ContractStatus.Accepted),  nameof(ContractStatus.Escrow),  nameof(ContractStatus.Pending) };
            var inactiveStatuses = new List<string> { nameof(ContractStatus.Rejected), nameof(ContractStatus.Expired) , nameof(ContractStatus.Draft),nameof(ContractStatus.Completed),nameof(ContractStatus.Cancelled)};
            var query = _context.ContractDetails.AsQueryable();

            // User-specific filtering
            if (request.UserId.HasValue)
            {
                var invitedContractIds = await _context.SellerBuyerInvitations
                    .Where(inv => inv.BuyerId == request.UserId || inv.SellerId == request.UserId || inv.CreatedBy == request.UserId.ToString())
                    .Select(inv => inv.ContractId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var teamMemberContracts = await _context.TeamMembers
                    .Where(tm => tm.UserId == request.UserId.ToString() && tm.IsActive == true && tm.IsDeleted != true && tm.ContractId != null)
                    .ToListAsync(cancellationToken);

                var teamContractIds = teamMemberContracts
                    .SelectMany(tm => tm.ContractId!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(id => id.Trim())
                    .Distinct()
                    .ToList();

                query = query.Where(c =>
                    invitedContractIds.Contains(c.Id) ||
                    c.CreatedBy == request.UserId.ToString() ||
                    teamContractIds.Contains(c.Id.ToString()));
            }

            if (request.Status.HasValue)
                query = query.Where(c => c.Status == request.Status.ToString());

            if (!string.IsNullOrEmpty(request.SearchKeyword))
                query = query.Where(c =>
                    c.ContractTitle.Contains(request.SearchKeyword) ||
                    c.Id.ToString() == request.SearchKeyword ||
                    c.BuyerMobile == request.SearchKeyword ||
                    c.SellerMobile == request.SearchKeyword);

            if (request.PriceFilter > 0)
                query = query.Where(c => c.FeeAmount >= request.PriceFilter);

            if (request.IsActive.HasValue)
            {
                query = request.IsActive.Value
                    ? query.Where(c => activeStatuses.Contains(c.Status))
                    : query.Where(c => inactiveStatuses.Contains(c.Status));
            }

            if (request.IsMilestone == true)
            {
                var contractIdsWithMilestones = await _context.MileStones
                    .Where(m => m.ContractId != null)
                    .Select(m => m.ContractId!)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                query = query.Where(c =>
                    contractIdsWithMilestones.Contains(c.Id) &&
                    activeStatuses.Contains(c.Status));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var contracts = await query
                .OrderByDescending(c => c.Created)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var contractIds = contracts.Select(c => c.Id.ToString()).ToList();

            var milestones = await _context.MileStones
                .Where(m => m.ContractId != null && contractIds.Contains(m.ContractId.ToString()!))
                .ToListAsync(cancellationToken);

            var invitations = await _context.SellerBuyerInvitations
                .Where(inv => contractIds.Contains(inv.ContractId.ToString()))
                .ToListAsync(cancellationToken);

            var teamMembers = await _context.TeamMembers
                .Where(tm => tm.ContractId != null && tm.IsDeleted != true)
                .ToListAsync(cancellationToken);

            var contractDTOs = contracts.Select(c => new ContractDetailsDTO
            {
                Id = c.Id,
                Role = c.Role ?? string.Empty,
                ContractTitle = c.ContractTitle,
                ServiceType = c.ServiceType,
                ServiceDescription = c.ServiceDescription,
                AdditionalNote = c.AdditionalNote,
                FeesPaidBy = c.FeesPaidBy,
                FeeAmount = c.FeeAmount,
                BuyerName = c.BuyerName,
                BuyerMobile = PhoneNumberHelper.ExtractPhoneNumberWithoutCountryCode(c.BuyerMobile),
                SellerName = c.SellerName,
                SellerMobile = PhoneNumberHelper.ExtractPhoneNumberWithoutCountryCode(c.SellerMobile),
                CreatedBy = c.CreatedBy,
                Status = c.Status,
                IsActive = c.IsActive,
                IsDeleted = c.IsDeleted,
                TaxAmount = c.TaxAmount,
                EscrowTax = c.EscrowTax,
                BuyerId = c.BuyerDetailsId?.ToString() ?? string.Empty,
                SellerId = c.SellerDetailsId?.ToString() ?? string.Empty,
                CountryCode = PhoneNumberHelper.ExtractCountryCode(c.BuyerMobile),
                Created = c.Created,
                ContractDoc = c.ContractDoc,
                LastModified = c.LastModified,
                LastModifiedBy = c.LastModifiedBy,
                SellerPayableAmount = c.SellerPayableAmount,
                BuyerPayableAmount = c.BuyerPayableAmount,

                MileStones = milestones
                    .Where(m => m.ContractId == c.Id)
                    .Select(m => new MileStoneDTO
                    {
                        Id = m.Id,
                        Name = m.Name ?? string.Empty,
                        DueDate = m.DueDate,
                        Amount = m.Amount,
                        Description = m.Description ?? string.Empty,
                        Documents = m.Documents ?? string.Empty,
                        ContractId = m.ContractId,
                        Status = m.Status
                    }).ToList(),

                InvitationDetails = invitations
                    .Where(inv => inv.ContractId == c.Id)
                    .Select(inv => new SellerBuyerInvitation
                    {
                        Id = inv.Id,
                        ContractId = inv.ContractId,
                        BuyerId = inv.BuyerId,
                        SellerId = inv.SellerId,
                        Status = inv.Status,
                        Created = inv.Created
                    }).FirstOrDefault(),

                TeamMembers = teamMembers
                    .Where(tm => tm.ContractId != null &&
                        tm.ContractId.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(id => id.Trim())
                            .Contains(c.Id.ToString()))
                    .Select(tm => new TeamDTO
                    {
                        TeamId = tm.Id.ToString(),
                        UserId = tm.UserId ?? string.Empty,
                        RoleType = tm.RoleType ?? string.Empty,
                        IsActive = tm.IsActive ?? false,
                        ContractId = tm.ContractId?
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(id => id.Trim())
                            .ToList() ?? new List<string>()
                    }).ToList()
            }).OrderByDescending(x => x.Created).ToList();

            return new PaginatedList<ContractDetailsDTO>(contractDTOs, totalCount, request.PageNumber, request.PageSize);
        }
    }
}







































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Helpers;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.Common.Models.ContractDTOs;
//using Escrow.Api.Domain.Entities.ContractPanel;
//using Escrow.Api.Domain.Enums;
//using MediatR;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Escrow.Api.Application.ContractPanel.ContractQueries
//{
//    public record GetContractsQuery : IRequest<PaginatedList<ContractDetailsDTO>>
//    {
//        public int? UserId { get; init; }
//        public ContractStatus? Status { get; init; }
//        public string? SearchKeyword { get; init; }
//        public bool? IsActive { get; init; }
//        public int? PriceFilter { get; init; }
//        public bool? IsMilestone { get; init; }
//        public int PageNumber { get; init; } = 1;
//        public int PageSize { get; init; } = 10;
//    }

//    public class GetContractsQueryHandler : IRequestHandler<GetContractsQuery, PaginatedList<ContractDetailsDTO>>
//    {
//        private readonly IApplicationDbContext _context;

//        public GetContractsQueryHandler(IApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<PaginatedList<ContractDetailsDTO>> Handle(GetContractsQuery request, CancellationToken cancellationToken)
//        {
//            List<string> activeStatuses = new() { nameof(ContractStatus.Accepted), nameof(ContractStatus.Completed), nameof(ContractStatus.Escrow) };
//            List<string> inactiveStatuses = new() { nameof(ContractStatus.Rejected), nameof(ContractStatus.Expired) };
//            var query = _context.ContractDetails.AsQueryable();

//            // Filter by user involvement

//            if (request.UserId.HasValue)
//            {
//                var invitedContractIds = await _context.SellerBuyerInvitations
//                    .Where(inv => inv.BuyerId == request.UserId || inv.SellerId == request.UserId || inv.CreatedBy == request.UserId.ToString())
//                    .Select(inv => inv.ContractId)
//                    .Distinct()
//                    .ToListAsync(cancellationToken);

//                var teamMemberContractIds = await _context.TeamMembers
//                    .Where(tm => tm.UserId == request.UserId.ToString() && tm.IsActive == true && tm.IsDeleted != true)
//                    .Select(tm => tm.ContractId)
//                    .Distinct()
//                    .ToListAsync(cancellationToken);

//                query = query.Where(c =>
//                        invitedContractIds.Contains(c.Id) ||
//                        c.CreatedBy == request.UserId.ToString() ||
//                        teamMemberContractIds.Contains(c.Id.ToString())); // <-- Added for Team Members
//            }


//            //// Filter by user involvementm
//            //if (request.UserId.HasValue)
//            //{
//            //    var invitedContractIds = await _context.SellerBuyerInvitations
//            //        .Where(inv => inv.BuyerId == request.UserId || inv.SellerId == request.UserId || inv.CreatedBy == request.UserId.ToString())
//            //        .Select(inv => inv.ContractId)
//            //        .Distinct()
//            //        .ToListAsync(cancellationToken);


//            //    query = query.Where(c =>
//            //            invitedContractIds.Contains(c.Id) ||
//            //            c.CreatedBy == request.UserId.ToString()); // <-- ADD THIS LINE
//            //    //query = query.Where(c => invitedContractIds.Contains(c.Id));
//            //}

//            if (request.Status.HasValue)
//                query = query.Where(c => c.Status == request.Status.ToString());

//            if (!string.IsNullOrEmpty(request.SearchKeyword))
//                query = query.Where(c => c.ContractTitle.Contains(request.SearchKeyword) || c.Id.ToString() == request.SearchKeyword || c.BuyerMobile == request.SearchKeyword || c.SellerMobile == request.SearchKeyword);

//            if (request.PriceFilter > 0)
//                query = query.Where(c => c.FeeAmount >= request.PriceFilter);


//            if (request.IsActive.HasValue)
//            {
//                if (request.IsActive.Value)
//                {
//                    query = query.Where(c => activeStatuses.Contains(c.Status));
//                }
//                else
//                {
//                    query = query.Where(c => inactiveStatuses.Contains(c.Status));
//                }
//            }

//            // ✅ Add this block
//            if (request.IsMilestone == true)
//            {
//                var contractIdsWithMilestones = await _context.MileStones
//                    .Select(m => m.ContractId)
//                    .Distinct()
//                    .ToListAsync(cancellationToken);

//                query = query.Where(c =>
//                    contractIdsWithMilestones.Contains(c.Id) &&
//                    activeStatuses.Contains(c.Status));
//            }


//            //if (request.IsActive.HasValue)
//            //    query = query.Where(c => c.IsActive == request.IsActive);

//            var totalCount = await query.CountAsync(cancellationToken);

//            var contracts = await query
//                .OrderByDescending(c => c.Created)
//                .Skip((request.PageNumber - 1) * request.PageSize)
//                .Take(request.PageSize)
//                .ToListAsync(cancellationToken);

//            var contractIds = contracts.Select(c => c.Id).ToList();

//            var milestones = await _context.MileStones
//                .Where(m => contractIds.Contains(Convert.ToInt32(m.ContractId)))
//                .ToListAsync(cancellationToken);

//            var invitations = await _context.SellerBuyerInvitations
//                .Where(inv => contractIds.Contains(inv.ContractId))
//                .ToListAsync(cancellationToken);

//            var contractDTOs = contracts.Select(c => new ContractDetailsDTO
//            {
//                Id = c.Id,
//                Role = c.Role ?? string.Empty,
//                ContractTitle = c.ContractTitle,
//                ServiceType = c.ServiceType,
//                ServiceDescription = c.ServiceDescription,
//                AdditionalNote = c.AdditionalNote,
//                FeesPaidBy = c.FeesPaidBy,
//                FeeAmount = c.FeeAmount,
//                BuyerName = c.BuyerName,
//                BuyerMobile = PhoneNumberHelper.ExtractPhoneNumberWithoutCountryCode(c.BuyerMobile),
//                SellerName = c.SellerName,
//                SellerMobile = PhoneNumberHelper.ExtractPhoneNumberWithoutCountryCode(c.SellerMobile),
//                CreatedBy = c.CreatedBy,
//                Status = c.Status,
//                IsActive = c.IsActive,
//                IsDeleted = c.IsDeleted,
//                TaxAmount = c.TaxAmount,
//                EscrowTax = c.EscrowTax,
//                BuyerId = c.BuyerDetailsId.ToString(),
//                SellerId = c.SellerDetailsId.ToString(),
//                CountryCode = PhoneNumberHelper.ExtractCountryCode(c.BuyerMobile),
//                Created = c.Created,
//                ContractDoc = c.ContractDoc,
//                LastModified = c.LastModified,
//                LastModifiedBy = c.LastModifiedBy,
//                SellerPayableAmount = c.SellerPayableAmount,
//                BuyerPayableAmount = c.BuyerPayableAmount,
//                MileStones = milestones
//                    .Where(m => m.ContractId == c.Id)
//                    .Select(m => new MileStoneDTO
//                    {
//                        Id = m.Id,
//                        Name = m.Name,
//                        DueDate = m.DueDate,
//                        Amount = m.Amount,
//                        Description = m.Description,
//                        Documents = m.Documents,
//                        ContractId = m.ContractId,
//                        Status = m.Status,
//                    }).ToList(),
//                InvitationDetails = invitations
//                    .Where(inv => inv.ContractId == c.Id)
//                    .Select(inv => new SellerBuyerInvitation
//                    {
//                        Id = inv.Id,
//                        ContractId = inv.ContractId,
//                        BuyerId = inv.BuyerId,
//                        SellerId = inv.SellerId,
//                        Status = inv.Status,
//                        Created = inv.Created
//                    }).FirstOrDefault()
//            }).OrderByDescending(x => x.Created).ToList();

//            return new PaginatedList<ContractDetailsDTO>(contractDTOs, totalCount, request.PageNumber, request.PageSize);
//        }
//    }
//}
