using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Escrow.Api.Application.Common.Helpers;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Escrow.Api.Application.ContractPanel.ContractQueries;

public record GetContractsQuery : IRequest<PaginatedList<ContractDetailsDTO>>
{
    public int? UserId { get; init; }
    public ContractStatus? Status { get; init; }
    public string? SearchKeyword { get; init; }
    public bool? IsActive { get; init; }
    public int? StartPrice { get; init; }
    public int? EndPrice { get; init; }
    public bool? IsMilestone { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetContractsQueryHandler : IRequestHandler<GetContractsQuery, PaginatedList<ContractDetailsDTO>>
{
    private readonly IConfiguration _configuration;

    public GetContractsQueryHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private IDbConnection CreateConnection()
    {
        var connectionString = _configuration.GetConnectionString("Escrow");
        var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    public async Task<PaginatedList<ContractDetailsDTO>> Handle(GetContractsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = CreateConnection();

            var json = await connection.ExecuteScalarAsync<string>(
                "SELECT get_contracts_json(" +
                    "@UserId, @Status, @SearchKeyword, @StartPrice, @EndPrice, " +
                    "@StartDate, @EndDate, @IsActive, @IsMilestone, @PageNumber, @PageSize" +
                ")",
                new
                {
                    UserId = request.UserId,
                    Status = request.Status?.ToString(),
                    SearchKeyword = request.SearchKeyword,
                    StartPrice = request.StartPrice,
                    EndPrice = request.EndPrice,
                    StartDate = request.StartDate?.Date,
                    EndDate = request.EndDate?.Date,
                    IsActive = request.IsActive,
                    IsMilestone = request.IsMilestone,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                });

            var contracts = string.IsNullOrEmpty(json)
                ? new List<ContractDetailsDTO>()
                : JsonSerializer.Deserialize<List<ContractDetailsDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return new PaginatedList<ContractDetailsDTO>(
                contracts ?? new(),
                contracts?.Count ?? 0,
                request.PageNumber,
                request.PageSize);
        }
        catch (Exception ex)
        {
            // You can inject and use a logger here if desired
            // _logger.LogError(ex, "Failed to load contracts");

            throw new ApplicationException("An error occurred while retrieving contracts.", ex);
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
