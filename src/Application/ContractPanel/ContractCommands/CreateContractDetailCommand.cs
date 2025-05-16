using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Escrow.Api.Application.DTOs;

namespace Escrow.Api.Application.ContractPanel.ContractCommands
{
    public record CreateContractDetailCommand : IRequest<int>
    {
        public string Role { get; set; } = string.Empty;
        public string ContractTitle { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public string? AdditionalNote { get; set; }
        public string FeesPaidBy { get; set; } = string.Empty;
        public decimal? FeeAmount { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerMobile { get; set; }
        public string? SellerName { get; set; }
        public string? SellerMobile { get; set; }
        public string? ContractDoc { get; set; }
        public string Status { get; set; } = "pending";
    }

    public class CreateContractDetailsHandler : IRequestHandler<CreateContractDetailCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly INotificationService _firebaseNotification;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateContractDetailsHandler(
            IApplicationDbContext applicationDbContext,
            IJwtService jwtService,
            INotificationService firebaseNotification,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = applicationDbContext;
            _jwtService = jwtService;
            _firebaseNotification = firebaseNotification;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> Handle(CreateContractDetailCommand request, CancellationToken cancellationToken)
{



            var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;
            int userId = _jwtService.GetUserId().ToInt();

            if (request.BuyerMobile == request.SellerMobile)
            {
                throw new InvalidOperationException(AppMessages.Get("BuyerAndSellerSame", language));
            }

            var monthlyLimitConfig = await _context.SystemConfigurations.FirstOrDefaultAsync(cancellationToken);
            decimal monthlyLimit = Convert.ToDecimal(monthlyLimitConfig?.Value ?? "0");

            // Ensure start and end of month are in UTC
            DateTime startOfMonth = new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1); // Inclusive end of the month

            decimal totalFeeAmountThisMonth = await _context.ContractDetails
                .Where(c => c.CreatedBy == userId.ToString()
                    && c.Created >= startOfMonth
                    && c.Created <= endOfMonth)
                .SumAsync(c => (decimal?)c.FeeAmount, cancellationToken) ?? 0m;

            decimal newFeeAmount = request.FeeAmount ?? 0;
            if ((totalFeeAmountThisMonth + newFeeAmount) > monthlyLimit)
            {
                throw new InvalidOperationException(AppMessages.Get("MonthlyContractLimitExceeded", language));
            }

            //var monthlyLimitConfig = await _context.SystemConfigurations.FirstOrDefaultAsync(cancellationToken);
            //decimal monthlyLimit = Convert.ToDecimal(monthlyLimitConfig?.Value ?? "0");

            //DateTime startOfMonth = new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            //DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            //decimal totalFeeAmountThisMonth = await _context.ContractDetails
            //    .Where(c => c.CreatedBy == userId.ToString() && c.Created >= startOfMonth && c.Created <= endOfMonth)
            //    .SumAsync(c => (decimal?)c.FeeAmount, cancellationToken) ?? 0m;




            //decimal newFeeAmount = request.FeeAmount ?? 0;
            //if ((totalFeeAmountThisMonth + newFeeAmount) > monthlyLimit)
            //{
            //    return Result<int>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("MonthlyContractLimitExceeded", language));
            //}

            int buyerId = await _firebaseNotification.GetOrCreateUserId(request.BuyerName, request.BuyerMobile, cancellationToken);
            int sellerId = await _firebaseNotification.GetOrCreateUserId(request.SellerName, request.SellerMobile, cancellationToken);

            var commission = await _context.CommissionMasters
                .Where(c => c.AppliedGlobally == true)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync(cancellationToken);

            decimal feeAmount = request.FeeAmount ?? 0;
            decimal escrowTax = commission?.CommissionRate ?? 0;
            decimal taxRate = commission?.TaxRate ?? 0;
            decimal escrowAmount = (feeAmount * escrowTax) / 100;
            decimal taxAmount = (escrowAmount * taxRate) / 100;
            decimal buyerPayableAmount = 0;
            decimal sellerPayableAmount = 0;

            switch (request.FeesPaidBy.ToLower())
            {
                case "buyer":
                    buyerPayableAmount = feeAmount + escrowAmount + taxAmount;
                    sellerPayableAmount = feeAmount;
                    break;
                case "seller":
                    sellerPayableAmount = feeAmount - taxAmount - escrowAmount;
                    buyerPayableAmount = feeAmount;
                    break;
                case "50":
                case "halfpayment":
                    buyerPayableAmount = feeAmount + (taxAmount / 2) + (escrowAmount / 2);
                    sellerPayableAmount = feeAmount - (taxAmount / 2) - (escrowAmount / 2);
                    break;
            }

            var entity = new ContractDetails
            {
                Role = request.Role,
                ContractTitle = request.ContractTitle,
                ServiceType = request.ServiceType,
                ServiceDescription = request.ServiceDescription,
                AdditionalNote = request.AdditionalNote,
                FeesPaidBy = request.FeesPaidBy,
                FeeAmount = feeAmount,
                BuyerName = request.BuyerName,
                BuyerMobile = request.BuyerMobile,
                SellerMobile = request.SellerMobile,
                SellerName = request.SellerName,
                Status = nameof(ContractStatus.Draft),
                ContractDoc = request.ContractDoc,
                BuyerDetailsId = buyerId,
                SellerDetailsId = sellerId,
                UserDetailId = userId,
                EscrowTax = escrowAmount,
                TaxAmount = taxAmount,
                BuyerPayableAmount = buyerPayableAmount.ToString(),
                SellerPayableAmount = sellerPayableAmount.ToString(),
                IsActive = true,
                IsDeleted = false
            };

            await _context.ContractDetails.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var log = new ContractDetailsLog
            {
                ContractId = entity.Id,
                Operation = "CREATE",
                ChangedFields = "All",
                PreviousData = null,
                NewData = JsonSerializer.Serialize(entity),
                Remarks = "Contract created by user.",
                Created = DateTime.UtcNow,
                ChangedBy = userId.ToString(),
                Source = "API"
            };

            await _context.ContractDetailsLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _firebaseNotification.SendNotificationAsync(userId, buyerId, sellerId, entity.Id, entity.Role, "Contract", cancellationToken);

            return entity.Id;

        }
    }
}















































//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using AutoMapper;
//using Escrow.Api.Application.Common.Constants;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Entities.ContractPanel;
//using Escrow.Api.Domain.Entities.Notifications;
//using Escrow.Api.Domain.Entities.UserPanel;
//using Escrow.Api.Domain.Enums;
//using MediatR;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;

//namespace Escrow.Api.Application.ContractPanel.ContractCommands
//{
//    public record CreateContractDetailCommand : IRequest<int>
//    {
//        public string Role { get; set; } = string.Empty;
//        public string ContractTitle { get; set; } = string.Empty;
//        public string ServiceType { get; set; } = string.Empty;
//        public string ServiceDescription { get; set; } = string.Empty;
//        public string? AdditionalNote { get; set; }
//        public string FeesPaidBy { get; set; } = string.Empty;
//        public decimal? FeeAmount { get; set; }
//        public string? BuyerName { get; set; }
//        public string? BuyerMobile { get; set; }
//        public string? SellerName { get; set; }
//        public string? SellerMobile { get; set; }
//        public string? ContractDoc { get; set; }
//        public string Status { get; set; } = "pending";
//    }

//    public class CreateContractDetailsHandler : IRequestHandler<CreateContractDetailCommand, int>
//    {
//        private readonly IApplicationDbContext _context;
//        private readonly IJwtService _jwtService;
//        private readonly INotificationService _firebaseNotification;

//        public CreateContractDetailsHandler(IApplicationDbContext applicationDbContext, IJwtService jwtService, INotificationService firebaseNotification)
//        {
//            _context = applicationDbContext;
//            _jwtService = jwtService;
//            _firebaseNotification = firebaseNotification;
//        }

//        public async Task<int> Handle(CreateContractDetailCommand request, CancellationToken cancellationToken)
//        {
//            int userId = _jwtService.GetUserId().ToInt();

//            if (request.BuyerMobile == request.SellerMobile)
//            {
//                throw new InvalidOperationException(AppMessages.BuyerandSellermobilenumberscannotbethesame);
//            }

//            var monthlyLimitConfig = await _context.SystemConfigurations.FirstOrDefaultAsync(cancellationToken);
//            decimal monthlyLimit = Convert.ToDecimal(monthlyLimitConfig?.Value ?? "0");

//            DateTime startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
//            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

//            decimal totalFeeAmountThisMonth = await _context.ContractDetails
//    .Where(c => c.CreatedBy == Convert.ToString(userId) &&
//                c.Created >= startOfMonth &&
//                c.Created <= endOfMonth)
//    .SumAsync(c => (decimal?)c.FeeAmount, cancellationToken) ?? 0m;


//            decimal newFeeAmount = request.FeeAmount ?? 0;
//            if ((totalFeeAmountThisMonth + newFeeAmount) > monthlyLimit)
//            {
//                throw new InvalidOperationException(AppMessages.MonthlyContractlimitexceeded);
//                // OR return Result<int>.Failure(StatusCodes.Status400BadRequest, "Monthly limit exceeded.");
//            }


//            //if (request.BuyerMobile == request.SellerMobile)
//            //{
//            //    return Result<int>.Failure(StatusCodes.Status400BadRequest,"Buyer and Seller mobile numbers cannot be the same.");
//            //}
//            // Get or create buyer/seller
//            int buyerId = await _firebaseNotification.GetOrCreateUserId(request.BuyerName, request.BuyerMobile, cancellationToken);
//            int sellerId = await _firebaseNotification.GetOrCreateUserId(request.SellerName, request.SellerMobile, cancellationToken);

//            // Get commission rule
//            var commission = await _context.CommissionMasters
//               .Where(c => c.AppliedGlobally || c.TransactionType == "Service")
//               .OrderByDescending(c => c.Id)
//               .FirstOrDefaultAsync(cancellationToken);

//            decimal feeAmount = request.FeeAmount ?? 0;
//            decimal escrowTax = commission?.CommissionRate ?? 0;
//            decimal taxRate = commission?.TaxRate ?? 0;
//            decimal escrowAmount = (feeAmount * escrowTax) / 100;
//            decimal taxAmount = (escrowAmount * taxRate) / 100;
//            decimal totalAmount = feeAmount + taxAmount + escrowAmount;

//            // Decide who pays the fee
//            decimal buyerPayableAmount = 0;
//            decimal sellerPayableAmount = 0;

//            if (request.FeesPaidBy.ToLower() == "buyer")
//            {
//                buyerPayableAmount = feeAmount + escrowAmount + taxAmount;
//                sellerPayableAmount = feeAmount;
//            }
//            else if (request.FeesPaidBy.ToLower() == "seller")
//            {
//                sellerPayableAmount = feeAmount - taxAmount - escrowAmount;
//                buyerPayableAmount = feeAmount;
//            }
//            else if (request.FeesPaidBy.ToLower() == "50" || request.FeesPaidBy.ToLower() == "halfPayment")
//            {
//                buyerPayableAmount = feeAmount + (taxAmount / 2) + (escrowAmount / 2);
//                sellerPayableAmount = feeAmount - (taxAmount / 2) - (escrowAmount / 2);
//            }

//            var entity = new ContractDetails
//            {
//                Role = request.Role,
//                ContractTitle = request.ContractTitle,
//                ServiceType = request.ServiceType,
//                ServiceDescription = request.ServiceDescription,
//                AdditionalNote = request.AdditionalNote,
//                FeesPaidBy = request.FeesPaidBy,
//                FeeAmount = feeAmount,
//                BuyerName = request.BuyerName,
//                BuyerMobile = request.BuyerMobile,
//                SellerMobile = request.SellerMobile,
//                SellerName = request.SellerName,
//                Status = nameof(ContractStatus.Draft),
//                ContractDoc = request.ContractDoc,
//                BuyerDetailsId = buyerId,
//                SellerDetailsId = sellerId,
//                UserDetailId = userId,
//                EscrowTax = escrowAmount,
//                TaxAmount = taxAmount,
//                BuyerPayableAmount = buyerPayableAmount.ToString(),
//                SellerPayableAmount = sellerPayableAmount.ToString(),
//                IsActive = true,
//                IsDeleted = false
//            };

//            await _context.ContractDetails.AddAsync(entity, cancellationToken);
//            await _context.SaveChangesAsync(cancellationToken);

//            // Log the creation
//            var log = new ContractDetailsLog
//            {
//                ContractId = entity.Id,
//                Operation = "CREATE",
//                ChangedFields = "All", // Or specify actual fields if needed
//                PreviousData = null,   // No previous data since it's a new record
//                NewData = System.Text.Json.JsonSerializer.Serialize(entity),
//                Remarks = "Contract created by user.",
//                Created = DateTime.UtcNow,
//                ChangedBy = userId.ToString(),
//                Source = "API"
//            };

//            await _context.ContractDetailsLogs.AddAsync(log, cancellationToken);
//            await _context.SaveChangesAsync(cancellationToken);

//            // Send Push Notification and Save Notification Record
//            await _firebaseNotification.SendNotificationAsync(userId, buyerId, sellerId, entity.Id, entity.Role, "Contract", cancellationToken);

//            return entity.Id;
//        }




//        //private async Task SendNotificationAsync(int creatorId, int buyerId, int sellerId, int contractId, string role, CancellationToken cancellationToken)
//        //{
//        //    // Fetch creator's name for use in notification message only (not stored)
//        //    var creatorName = await _context.UserDetails
//        //        .Where(u => u.Id == creatorId)
//        //        .Select(u => u.FullName)
//        //        .FirstOrDefaultAsync(cancellationToken);

//        //    // Fetch buyer and seller user data
//        //    var users = await _context.UserDetails
//        //        .Where(u => u.Id == buyerId || u.Id == sellerId) // Get buyer and seller records
//        //        .ToListAsync(cancellationToken);

//        //    // Filter out the record that does not match creatorId
//        //    var userToNotify = users.FirstOrDefault(u => u.Id != creatorId); // This will give you the user who is not the creator

//        //    // If we have a user to notify, proceed with the notification logic
//        //    if (userToNotify != null)
//        //    {
//        //        var userInfo = new { userToNotify.FullName, userToNotify.DeviceToken, userToNotify.IsNotified };

//        //        // Create and send the notification
//        //        var title = "New Contract Created";
//        //        var description = $"{creatorName} has created a new contract for you, {userInfo.FullName}. Please review the details.";

//        //        // Save notification to DB (note: FromID is NOT added here)
//        //        var notification = new Notification
//        //        {
//        //            ToID = userToNotify.Id,
//        //            ContractId = contractId,
//        //            Title = title,
//        //            Description = description,
//        //            Type = "Contract",
//        //            IsRead = false,
//        //            Created = DateTime.UtcNow,
//        //        };

//        //        // Save the notification in the database
//        //        await _context.Notifications.AddAsync(notification, cancellationToken);

//        //        // Send push notification if applicable
//        //        if (!string.IsNullOrEmpty(userInfo.DeviceToken) && userInfo.IsNotified == true)
//        //        {
//        //            await _firebaseNotification.SendPushNotificationAsync(
//        //                userInfo.DeviceToken,
//        //                title,
//        //                description,
//        //                new { ContractId = contractId, Type = "Contract", Role = role }
//        //            );
//        //        }

//        //        // Save changes to the database
//        //        await _context.SaveChangesAsync(cancellationToken);
//        //    }
//        //}


//        //private async Task<int> GetOrCreateUserId(string? name, string? mobile, CancellationToken cancellationToken)
//        //{
//        //    if (string.IsNullOrEmpty(mobile)) return 0;

//        //    var user = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == mobile, cancellationToken);

//        //    if (user != null)
//        //    {
//        //        return user.Id;
//        //    }

//        //    var newUser = new UserDetail
//        //    {
//        //        UserId = Guid.NewGuid().ToString(),
//        //        FullName = name ?? "Unknown",
//        //        PhoneNumber = mobile,
//        //        Created = DateTime.UtcNow,
//        //        IsActive = true,
//        //        IsDeleted = false,
//        //        IsProfileCompleted = false,
//        //        Role = nameof(Roles.User),
//        //    };

//        //    _context.UserDetails.Add(newUser);
//        //    await _context.SaveChangesAsync(cancellationToken);

//        //    return newUser.Id;
//        //}
//    }
//}
