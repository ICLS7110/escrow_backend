using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.ContractPanel;
using Escrow.Api.Domain.Entities.Notifications;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        public CreateContractDetailsHandler(IApplicationDbContext applicationDbContext, IJwtService jwtService, INotificationService firebaseNotification)
        {
            _context = applicationDbContext;
            _jwtService = jwtService;
            _firebaseNotification = firebaseNotification;
        }

        public async Task<int> Handle(CreateContractDetailCommand request, CancellationToken cancellationToken)
        {
            int userId = _jwtService.GetUserId().ToInt();

            // Get or create buyer/seller
            int buyerId = await GetOrCreateUserId(request.BuyerName, request.BuyerMobile, cancellationToken);
            int sellerId = await GetOrCreateUserId(request.SellerName, request.SellerMobile, cancellationToken);

            // Get commission rule
            var commission = await _context.CommissionMasters
                .Where(c => c.AppliedGlobally || c.TransactionType == request.ServiceType)
                .OrderByDescending(c => c.AppliedGlobally)
                .FirstOrDefaultAsync(cancellationToken);

            decimal feeAmount = request.FeeAmount ?? 0;
            decimal escrowTax = commission?.CommissionRate ?? 0;
            decimal taxRate = commission?.TaxRate ?? 0;
            decimal escrowAmount = (feeAmount * escrowTax) / 100;
            decimal taxAmount = (escrowAmount * taxRate) / 100;
            decimal totalAmount = feeAmount + taxAmount + escrowAmount;

            // Decide who pays the fee
            decimal buyerPayableAmount = 0;
            decimal sellerPayableAmount = 0;

            if (request.FeesPaidBy.ToLower() == "buyer")
            {
                buyerPayableAmount = feeAmount + escrowAmount + taxAmount;
                sellerPayableAmount = feeAmount;
            }
            else if (request.FeesPaidBy.ToLower() == "seller")
            {
                sellerPayableAmount = feeAmount - taxAmount - escrowAmount;
                buyerPayableAmount = feeAmount;
            }
            else if (request.FeesPaidBy.ToLower() == "50" || request.FeesPaidBy.ToLower() == "halfPayment")
            {
                buyerPayableAmount = feeAmount + (taxAmount / 2) + (escrowAmount / 2);
                sellerPayableAmount = feeAmount - (taxAmount / 2) - (escrowAmount / 2);
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

            // Log the creation
            var log = new ContractDetailsLog
            {
                ContractId = entity.Id,
                Operation = "CREATE",
                ChangedFields = "All", // Or specify actual fields if needed
                PreviousData = null,   // No previous data since it's a new record
                NewData = System.Text.Json.JsonSerializer.Serialize(entity),
                Remarks = "Contract created by user.",
                Created = DateTime.UtcNow,
                ChangedBy = userId.ToString(),
                Source = "API"
            };

            await _context.ContractDetailsLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Send Push Notification and Save Notification Record
            await SendNotificationAsync(userId, buyerId, sellerId, entity.Id, entity.Role, cancellationToken);

            return entity.Id;
        }

        private async Task SendNotificationAsync(int creatorId, int buyerId, int sellerId, int contractId, string role, CancellationToken cancellationToken)
        {
            var creator = await _context.UserDetails
                .Where(u => u.Id == creatorId)
                .Select(u => new { u.Id, u.FullName, u.IsNotified })
                .FirstOrDefaultAsync(cancellationToken);

            var users = await _context.UserDetails
                .Where(u => u.Id == buyerId || u.Id == sellerId)
                .ToDictionaryAsync(u => u.Id, u => new { u.FullName, u.DeviceToken, u.IsNotified }, cancellationToken);

            var notifications = new List<Notification>();
            if (users.TryGetValue(buyerId, out var buyerInfo))
            {
                var title = "New Contract Created";
                var description = $"{creator?.FullName} has created a new contract for you, {buyerInfo.FullName}. Please review the details.";

                notifications.Add(new Notification
                {
                    FromID = creatorId,
                    ToID = buyerId,
                    ContractId = contractId,
                    Title = title,
                    Description = description,
                    Type = "Contract",
                    IsRead = false
                });

                if (!string.IsNullOrEmpty(buyerInfo.DeviceToken) && buyerInfo.IsNotified == true)
                {
                    await _firebaseNotification.SendPushNotificationAsync(
                        buyerInfo.DeviceToken!,
                        title,
                        description,
                        new { ContractId = contractId, Type = "Contract", Role = role }
                    );
                }

            }


            //if (users.TryGetValue(buyerId, out var buyerInfo))
            //{
            //    var title = "New Contract Created";
            //    var description = $"{creator?.FullName} has created a new contract for you, {buyerInfo.FullName}. Please review the details.";

            //    notifications.Add(new Notification
            //    {
            //        FromID = creatorId,
            //        ToID = buyerId,
            //        ContractId = contractId,
            //        Title = title,
            //        Description = description,
            //        Type = "Contract",
            //        IsRead = false
            //    });

            //    if (!string.IsNullOrEmpty(buyerInfo.DeviceToken))
            //    {
            //        await _firebaseNotification.SendPushNotificationAsync(buyerInfo.DeviceToken, title, description, new { ContractId = contractId, Type = "Contract", Role = role });
            //    }
            //}

            if (users.TryGetValue(sellerId, out var sellerInfo))
            {
                var title = "New Contract Created";
                var description = $"{creator?.FullName} has created a new contract for you, {sellerInfo.FullName}. Please review the details.";

                notifications.Add(new Notification
                {
                    FromID = creatorId,
                    ToID = sellerId,
                    ContractId = contractId,
                    Title = title,
                    Description = description,
                    Type = "Contract",
                    IsRead = false
                });

                if (!string.IsNullOrEmpty(sellerInfo.DeviceToken) && sellerInfo.IsNotified == true)
                {
                    await _firebaseNotification.SendPushNotificationAsync(sellerInfo.DeviceToken, title, description, new { ContractId = contractId, Type = "Contract", Role = role });
                }
            }

            if (notifications.Any())
            {
                await _context.Notifications.AddRangeAsync(notifications, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }



        //Working Send Notification Code 

        //private async Task SendNotificationAsync(int creatorId, int buyerId, int sellerId, int contractId, string Role, CancellationToken cancellationToken)
        //{
        //    var title = "New Contract Created";
        //    var description = "A new contract has been created. Please check the details.";

        //    var users = await _context.UserDetails
        //        .Where(u => u.Id == buyerId || u.Id == sellerId)
        //        .ToDictionaryAsync(u => u.Id, u => u.DeviceToken, cancellationToken);

        //    var notifications = new List<Notification>();

        //    if (users.TryGetValue(buyerId, out var buyerToken))
        //    {
        //        notifications.Add(new Notification
        //        {
        //            FromID = creatorId,
        //            ToID = buyerId,
        //            ContractId = contractId,
        //            Title = title,
        //            Description = description,
        //            Type = "Contract",
        //            IsRead = false
        //        });
        //        //await _firebaseNotification.SendPushNotificationAsync(buyerToken, title, description);
        //        if (!string.IsNullOrEmpty(buyerToken))
        //        {
        //            await _firebaseNotification.SendPushNotificationAsync(buyerToken, title, description, new { ContractId = contractId, Type = "Contract", Role = Role });
        //        }
        //    }
        //    try
        //    {
        //        if (users.TryGetValue(sellerId, out var sellerToken))
        //        {
        //            notifications.Add(new Notification
        //            {
        //                FromID = creatorId,
        //                ToID = sellerId,
        //                ContractId = contractId,
        //                Title = title,
        //                Description = description,
        //                Type = "Contract",
        //                IsRead = false
        //            });
        //            if (!string.IsNullOrEmpty(sellerToken))
        //            {
        //                await _firebaseNotification.SendPushNotificationAsync(sellerToken, title, description, new { ContractId = contractId, Type = "Contract", Role = Role });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        var x= ex;
        //    }

        //    if (notifications.Any())
        //    {
        //        await _context.Notifications.AddRangeAsync(notifications, cancellationToken);
        //        await _context.SaveChangesAsync(cancellationToken);
        //    }
        //}











        //private async Task SendNotificationAsync(int creatorId, int buyerId, int sellerId, int contractId, CancellationToken cancellationToken)
        //{
        //    var toUserId = creatorId == buyerId ? sellerId : buyerId;
        //    var fromUserId = creatorId;

        //    var toUser = await _context.UserDetails.FirstOrDefaultAsync(u => u.Id == toUserId, cancellationToken);

        //    if (toUser != null && !string.IsNullOrEmpty(toUser.DeviceToken))
        //    {
        //        string title = "New Contract Created";
        //        string description = "A new contract has been created. Please check the details.";

        //        await _firebaseNotification.SendPushNotificationAsync(toUser.DeviceToken, title, description);

        //        var notification = new Notification
        //        {
        //            FromID = fromUserId,
        //            ToID = toUserId,
        //            ContractId = contractId,
        //            Title = title,
        //            Description = description,
        //            Type = "Contract"
        //        };

        //        await _context.Notifications.AddAsync(notification, cancellationToken);
        //        await _context.SaveChangesAsync(cancellationToken);
        //    }
        //}
        private async Task<int> GetOrCreateUserId(string? name, string? mobile, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(mobile)) return 0;

            var user = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == mobile, cancellationToken);

            if (user != null)
            {
                return user.Id;
            }

            var newUser = new UserDetail
            {
                UserId = Guid.NewGuid().ToString(),
                FullName = name ?? "Unknown",
                PhoneNumber = mobile,
                Created = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
                IsProfileCompleted = false,
                Role = nameof(Roles.User),
            };

            _context.UserDetails.Add(newUser);
            await _context.SaveChangesAsync(cancellationToken);

            return newUser.Id;
        }
    }
}










































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Amazon.Runtime.Internal;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models.ContractDTOs;
//using Escrow.Api.Domain.Entities.ContractPanel;
//using Escrow.Api.Domain.Entities.UserPanel;
//using Escrow.Api.Domain.Enums;

//namespace Escrow.Api.Application.ContractPanel.ContractCommands;
//public record CreateContractDetailCommand : IRequest<int>
//{
//    public string Role { get; set; } = string.Empty;
//    public string ContractTitle { get; set; } = string.Empty;
//    public string ServiceType { get; set; } = string.Empty;
//    public string ServiceDescription { get; set; } = string.Empty;
//    public string? AdditionalNote { get; set; }
//    public string FeesPaidBy { get; set; } = string.Empty;
//    public decimal? FeeAmount { get; set; }
//    public string? BuyerName { get; set; }
//    public string? BuyerMobile { get; set; }
//    public string? SellerName { get; set; }
//    public string? SellerMobile { get; set; }
//    public string? ContractDoc { get; set; }
//    public string Status { get; set; } = "pending";

//    //public List<MileStoneDTO>? MileStones { get; set; }
//}
//public class CreateContractDetailsHandler : IRequestHandler<CreateContractDetailCommand, int>
//{
//    private readonly IApplicationDbContext _context;
//   private readonly IJwtService _jwtService;
//    private readonly IMapper _mapper;

//    public CreateContractDetailsHandler(IApplicationDbContext applicationDbContext, IJwtService jwtService, IMapper mapper)
//    {
//        _context = applicationDbContext;
//        _jwtService = jwtService;
//        _mapper = mapper;
//    }

//    public async Task<int> Handle(CreateContractDetailCommand request, CancellationToken cancellationToken)
//    {
//        int userId = _jwtService.GetUserId().ToInt();

//        int buyerId = await GetOrCreateUserId(request.BuyerName, request.BuyerMobile, cancellationToken);
//        int sellerId = await GetOrCreateUserId(request.SellerName, request.SellerMobile, cancellationToken);

//        var entity = new ContractDetails
//        {
//            Role = request.Role,
//            ContractTitle = request.ContractTitle,
//            ServiceType = request.ServiceType,
//            ServiceDescription = request.ServiceDescription,
//            AdditionalNote = request.AdditionalNote,
//            FeesPaidBy = request.FeesPaidBy,
//            FeeAmount = request.FeeAmount,
//            BuyerName = request.BuyerName,
//            BuyerMobile = request.BuyerMobile,
//            SellerMobile = request.SellerMobile,
//            SellerName = request.SellerName,
//            Status = nameof(ContractStatus.Draft),
//            ContractDoc = request.ContractDoc,
//            BuyerDetailsId = buyerId,
//            SellerDetailsId = sellerId,
//            UserDetailId = userId
//        };

//        await _context.ContractDetails.AddAsync(entity);
//        await _context.SaveChangesAsync(cancellationToken);

//        return entity.Id;
//    }

//    private async Task<int> GetOrCreateUserId(string? name, string? mobile, CancellationToken cancellationToken)
//    {
//        if (string.IsNullOrEmpty(mobile)) return 0;

//        var user = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == mobile, cancellationToken);

//        if (user != null)
//        {
//            return user.Id;
//        }

//        var newUser = new UserDetail
//        {
//            UserId = Guid.NewGuid().ToString(),
//            FullName = name ?? "Unknown",
//            PhoneNumber = mobile,
//            Created = DateTime.UtcNow,
//            IsActive = true,
//            IsDeleted = false
//        };

//        _context.UserDetails.Add(newUser);
//        await _context.SaveChangesAsync(cancellationToken);

//        return newUser.Id;
//    }
//}



////public class CreateContractDetailsHandler : IRequestHandler<CreateContractDetailCommand, int>
////{
////    private readonly IApplicationDbContext _context;
////    private readonly IJwtService _jwtService;
////    private readonly IMapper _mapper;
////    public CreateContractDetailsHandler(IApplicationDbContext applicationDbContext, IJwtService jwtService, IMapper mapper)
////    {
////        _context = applicationDbContext;
////        _jwtService = jwtService;
////        _mapper = mapper;
////    }

////    public async Task<int> Handle(CreateContractDetailCommand request, CancellationToken cancellationToken)
////    {
////        int ContractId;
////        int userid = _jwtService.GetUserId().ToInt();
////        var entity = new ContractDetails
////        {
////            Role = request.Role,
////            ContractTitle = request.ContractTitle,
////            ServiceType = request.ServiceType,
////            ServiceDescription = request.ServiceDescription,
////            AdditionalNote = request.AdditionalNote,
////            FeesPaidBy = request.FeesPaidBy,
////            FeeAmount = request.FeeAmount,
////            BuyerName = request.BuyerName,
////            BuyerMobile = request.BuyerMobile,
////            SellerMobile = request.SellerMobile,
////            SellerName = request.SellerName,
////            Status = request.Status,
////            ContractDoc = request.ContractDoc,
////            BuyerDetailsId = request.Role == EscrowApIConstant.ContratConstant.ContractRoleBuyer ? userid : null,
////            SellerDetailsId = request.Role == EscrowApIConstant.ContratConstant.ContractRoleSeller ? userid : null,
////            UserDetailId = userid
////        };
////        await _context.ContractDetails.AddAsync(entity);
////        await _context.SaveChangesAsync(cancellationToken);
////        ContractId = entity.Id;
////        await _context.SaveChangesAsync(cancellationToken);
////        return ContractId;
////    }
////}
