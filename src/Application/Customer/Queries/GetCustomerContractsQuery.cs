using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models.ContractDTOs;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Domain.Entities.ContractPanel;
using System.Linq.Expressions;
using Escrow.Api.Application.Customer.Queries;
using Escrow.Api.Application.Common.Constants;

namespace Escrow.Api.Application.Customer.Queries
{
    public class GetCustomerContractsQuery : IRequest<Result<CustomerContractDetailsDto>>
    {
        public int ActivePageNumber { get; set; } = 1;
        public int HistoricalPageNumber { get; set; } = 1;
        public int ActivePageSize { get; set; } = 10;
        public int HistoricalPageSize { get; set; } = 10;
        public string? CustomerId { get; set; }
    }



    public class GetCustomerContractsQueryHandler : IRequestHandler<GetCustomerContractsQuery, Result<CustomerContractDetailsDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly List<string> _activeStatuses = new() { nameof(ContractStatus.Accepted), nameof(ContractStatus.Escrow), nameof(ContractStatus.Pending), nameof(ContractStatus.Draft) };
        private readonly List<string> _inactiveStatuses = new() { nameof(ContractStatus.Rejected), nameof(ContractStatus.Expired), nameof(ContractStatus.Completed) };

        public GetCustomerContractsQueryHandler(IApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<CustomerContractDetailsDto>> Handle(GetCustomerContractsQuery request, CancellationToken cancellationToken)
        {
            // Get the current language (defaults to English if none provided)
            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;

            var baseQuery = _context.ContractDetails
                .AsNoTracking()
                .Include(c => c.MileStones)
                .Where(c => c.IsDeleted != true);

            if (request.CustomerId != null)
            {
                baseQuery = baseQuery.Where(c =>
                    c.BuyerDetailsId.ToString() == request.CustomerId ||
                    c.SellerDetailsId.ToString() == request.CustomerId ||
                    c.CreatedBy == request.CustomerId);
            }

            var projectedQuery = baseQuery
                .Select(contract => new ContractDetailsDTO
                {
                    Id = contract.Id,
                    ContractTitle = contract.ContractTitle,
                    ServiceDescription = contract.ServiceDescription,
                    Status = contract.Status,
                    ServiceType = contract.ServiceType ?? "N/A",
                    Created = contract.Created,
                    FeeAmount = contract.FeeAmount,
                    FeesPaidBy = contract.FeesPaidBy ?? "N/A",
                    BuyerName = contract.BuyerName ?? "N/A",
                    BuyerMobile = contract.BuyerMobile ?? "N/A",
                    BuyerId = contract.BuyerDetailsId.ToString(),
                    SellerName = contract.SellerName ?? "N/A",
                    SellerMobile = contract.SellerMobile ?? "N/A",
                    SellerId = contract.SellerDetailsId.ToString(),
                    CreatedBy = contract.CreatedBy ?? "N/A",
                    ContractDoc = contract.ContractDoc ?? "N/A",
                    IsActive = contract.IsActive ?? false,
                    IsDeleted = contract.IsDeleted ?? false,
                    TaxAmount = contract.TaxAmount ?? 0,
                    EscrowTax = contract.EscrowTax ?? 0,
                    LastModifiedBy = contract.LastModifiedBy ?? "N/A",
                    BuyerPayableAmount = contract.BuyerPayableAmount ?? "0",
                    SellerPayableAmount = contract.SellerPayableAmount ?? "0",
                    MileStones = contract.MileStones != null
                        ? contract.MileStones.Select(m => new MileStoneDTO
                        {
                            Id = m.Id,
                            Name = m.Name,
                            Amount = m.Amount
                        }).ToList()
                        : new List<MileStoneDTO>()
                });

            var activeContracts = await PaginatedList<ContractDetailsDTO>.CreateAsync(
                projectedQuery.Where(c => _activeStatuses.Contains(c.Status)).OrderByDescending(c => c.Created),
                request.ActivePageNumber,
                request.ActivePageSize);

            var historicalContracts = await PaginatedList<ContractDetailsDTO>.CreateAsync(
                projectedQuery.Where(c => _inactiveStatuses.Contains(c.Status)).OrderByDescending(c => c.Created),
                request.HistoricalPageNumber,
                request.HistoricalPageSize);

            if (activeContracts.TotalCount == 0 && historicalContracts.TotalCount == 0)
            {
                return Result<CustomerContractDetailsDto>.Failure(StatusCodes.Status404NotFound, AppMessages.Get("NoContractsFound", language));
            }

            var response = new CustomerContractDetailsDto
            {
                ActiveContracts = activeContracts.Items.ToList(),
                HistoricalContracts = historicalContracts.Items.ToList(),
                ActiveContractsTotal = activeContracts.TotalCount,
                HistoricalContractsTotal = historicalContracts.TotalCount
            };

            return Result<CustomerContractDetailsDto>.Success(StatusCodes.Status200OK, AppMessages.Get("ContractsRetrieved", language), response);
        }
    }
}



//public async Task<Result<CustomerContractDetailsDto>> Handle(GetCustomerContractsQuery request, CancellationToken cancellationToken)
//{
//    // Define statuses for active and historical contracts
//    //var activeStatuses = new[] { "Created", "InProgress" };
//    //var historicalStatuses = new[] { "Completed", "Cancelled" };

//    var baseQuery = _context.ContractDetails
//        .AsNoTracking()
//        .Include(c => c.MileStones)
//        .Where(c => c.IsDeleted != true);

//    if (request.CustomerId != null)
//    {
//        baseQuery = baseQuery.Where(c => c.BuyerDetailsId.ToString() == request.CustomerId || c.SellerDetailsId.ToString() == request.CustomerId || c.CreatedBy == request.CustomerId);
//    }

//    var projectedQuery = baseQuery
//        .Select(contract => new ContractDetailsDTO
//        {
//            Id = contract.Id,
//            ContractTitle = contract.ContractTitle,
//            ServiceDescription = contract.ServiceDescription,
//            Status = contract.Status,
//            ServiceType = contract.ServiceType ?? "N/A",
//            Created = contract.Created,
//            FeeAmount = contract.FeeAmount,
//            FeesPaidBy = contract.FeesPaidBy ?? "N/A",
//            BuyerName = contract.BuyerName ?? "N/A",
//            BuyerMobile = contract.BuyerMobile ?? "N/A",
//            BuyerId = contract.BuyerDetailsId.ToString(),
//            SellerName = contract.SellerName ?? "N/A",
//            SellerMobile = contract.SellerMobile ?? "N/A",
//            SellerId = contract.SellerDetailsId.ToString(),
//            CreatedBy = contract.CreatedBy ?? "N/A",
//            ContractDoc = contract.ContractDoc ?? "N/A",
//            IsActive = contract.IsActive ?? false,
//            IsDeleted = contract.IsDeleted ?? false,
//            TaxAmount = contract.TaxAmount ?? 0,
//            EscrowTax = contract.EscrowTax ?? 0,
//            LastModifiedBy = contract.LastModifiedBy ?? "N/A",
//            BuyerPayableAmount = contract.BuyerPayableAmount ?? "0",
//            SellerPayableAmount = contract.SellerPayableAmount ?? "0",
//            MileStones = contract.MileStones != null
//                ? contract.MileStones.Select(m => new MileStoneDTO
//                {
//                    Id = m.Id,
//                    Name = m.Name,
//                    Amount = m.Amount
//                }).ToList()
//                : new List<MileStoneDTO>()
//        });

//    var activeContracts = await PaginatedList<ContractDetailsDTO>.CreateAsync(
//        projectedQuery.Where(c => _activeStatuses.Contains(c.Status)).OrderByDescending(c => c.Created),
//        request.ActivePageNumber,
//        request.ActivePageSize);

//    var historicalContracts = await PaginatedList<ContractDetailsDTO>.CreateAsync(
//        projectedQuery.Where(c => _inactiveStatuses.Contains(c.Status)).OrderByDescending(c => c.Created),
//        request.HistoricalPageNumber,
//        request.HistoricalPageSize);

//    // Check if no contracts found
//    if (activeContracts.TotalCount == 0 && historicalContracts.TotalCount == 0)
//    {
//        return Result<CustomerContractDetailsDto>.Failure(StatusCodes.Status404NotFound, "No contracts found for the given customer.");
//    }

//    // Build response object
//    var response = new CustomerContractDetailsDto
//    {
//        ActiveContracts = activeContracts.Items.ToList(),
//        HistoricalContracts = historicalContracts.Items.ToList(),
//        ActiveContractsTotal = activeContracts.TotalCount,
//        HistoricalContractsTotal = historicalContracts.TotalCount
//    };

//    // Return success response
//    return Result<CustomerContractDetailsDto>.Success(StatusCodes.Status200OK, "Contracts retrieved successfully.", response);
//}
