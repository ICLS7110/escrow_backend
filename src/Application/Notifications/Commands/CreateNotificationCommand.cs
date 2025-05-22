using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Constants;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.Notifications;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.Notifications.Commands;

public class CreateNotificationCommand : IRequest<Result<object>>
{
    public string BuyerPhoneNumber { get; set; } = string.Empty;
    public string SellerPhoneNumber { get; set; } = string.Empty;
    public int ContractId { get; set; }
    public string? GroupId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<object>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateNotificationCommandHandler(
        IApplicationDbContext context,
        INotificationService notificationService,
        IJwtService jwtService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _notificationService = notificationService;
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<object>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var language = _httpContextAccessor.HttpContext?.GetCurrentLanguage() ?? Language.English;
        var currentUserId = _jwtService.GetUserId();

        if (string.IsNullOrWhiteSpace(request.BuyerPhoneNumber) || string.IsNullOrWhiteSpace(request.SellerPhoneNumber))
            return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("PhoneNumberRequired", language));

        if (request.ContractId <= 0)
            return Result<object>.Failure(StatusCodes.Status400BadRequest, AppMessages.Get("InvalidContractId", language));


        // Fetch contract title
        var contract = await _context.ContractDetails
            .Where(c => c.Id == request.ContractId)
            .Select(c => new { c.Id, c.ContractTitle })
            .FirstOrDefaultAsync(cancellationToken);

        var contractTitle = contract?.ContractTitle ?? "Contract";
        

        var buyer = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == request.BuyerPhoneNumber, cancellationToken);
        if (buyer == null)
        {
            buyer = new UserDetail
            {
                UserId = Guid.NewGuid().ToString(),
                PhoneNumber = request.BuyerPhoneNumber,
                Created = DateTime.UtcNow,
                Role = nameof(Roles.User),
                RecordState = RecordState.Active
            };
            _context.UserDetails.Add(buyer);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var seller = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == request.SellerPhoneNumber, cancellationToken);
        if (seller == null)
        {
            seller = new UserDetail
            {
                UserId = Guid.NewGuid().ToString(),
                PhoneNumber = request.SellerPhoneNumber,
                Created = DateTime.UtcNow,
                Role = nameof(Roles.User),
                RecordState = RecordState.Active
            };
            _context.UserDetails.Add(seller);
            await _context.SaveChangesAsync(cancellationToken);
        }

        int fromId, toId;
        string? deviceToken = null;

        if (buyer.Id.ToString() == currentUserId)
        {
            fromId = buyer.Id;
            toId = seller.Id;
            deviceToken = seller.DeviceToken;
        }
        else
        {
            fromId = seller.Id;
            toId = buyer.Id;
            deviceToken = buyer.DeviceToken;
        }

        var notification = new Domain.Entities.Notifications.Notification
        {
            FromID = fromId,
            ToID = toId,
            ContractId = request.ContractId,
            GroupId = request.GroupId,
            Type = request.Type,
            Title = request.Title,
            Description = request.Description,
            Created = DateTimeOffset.UtcNow,
            CreatedBy = currentUserId,
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = currentUserId,
            IsRead = false,
            RecordState = RecordState.Active
        };

        await _context.Notifications.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);


        var receiver = toId == buyer.Id ? buyer : seller;
        var receiverName = receiver.FullName ?? "User";
        var receiverImage = receiver.ProfilePicture ?? "";

        if (!string.IsNullOrEmpty(deviceToken))
        {
            await _notificationService.SendPushNotificationAsync(
                                        deviceToken,
                                        request.Title,
                                        request.Description,
                                        new
                                        {
                                            GroupId = request.GroupId,
                                            ContractId = request.ContractId,
                                            ContractTitle = contractTitle,
                                            ReceiverName = receiverName,
                                            ReceiverImage = receiverImage,
                                            Type = request.Type
                                        });
            await _notificationService.SendPushNotificationAsync(deviceToken, request.Title, request.Description, new { Type = request.Type });
        }

        return Result<object>.Success(StatusCodes.Status200OK, AppMessages.Get("NotificationSentSuccessfully", language));
    }
}










































//using System;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Entities.Notifications;
//using Escrow.Api.Domain.Entities.UserPanel;
//using Escrow.Api.Domain.Enums;
//using MediatR;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;

//namespace Escrow.Api.Application.Notifications.Commands
//{
//    public class CreateNotificationCommand : IRequest<Result<object>>
//    {
//        public string BuyerPhoneNumber { get; set; } = string.Empty;
//        public string SellerPhoneNumber { get; set; } = string.Empty;
//        public int ContractId { get; set; }
//        public string? GroupId { get; set; }
//        public string Type { get; set; } = string.Empty;
//        public string Title { get; set; } = string.Empty;
//        public string Description { get; set; } = string.Empty;
//    }

//    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<object>>
//    {
//        private readonly IApplicationDbContext _context;
//        private readonly INotificationService _notificationService;
//        private readonly IJwtService _jwtService;

//        public CreateNotificationCommandHandler(
//            IApplicationDbContext context,
//            INotificationService notificationService,
//            IJwtService jwtService)
//        {
//            _context = context ?? throw new ArgumentNullException(nameof(context));
//            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
//            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
//        }


//        public async Task<Result<object>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
//        {
//            try
//            {
//                if (string.IsNullOrWhiteSpace(request.BuyerPhoneNumber) || string.IsNullOrWhiteSpace(request.SellerPhoneNumber))
//                    return Result<object>.Failure(StatusCodes.Status400BadRequest, "Phone numbers are required.");

//                if (request.ContractId <= 0)
//                    return Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid ContractId.");

//                var currentUserId = _jwtService.GetUserId();

//                // Get or create Buyer
//                var buyer = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == request.BuyerPhoneNumber, cancellationToken);
//                if (buyer == null)
//                {
//                    buyer = new UserDetail
//                    {
//                        UserId = Guid.NewGuid().ToString(),
//                        PhoneNumber = request.BuyerPhoneNumber,
//                        Created = DateTime.UtcNow,
//                        Role = nameof(Roles.User),
//                    };
//                    _context.UserDetails.Add(buyer);
//                    await _context.SaveChangesAsync(cancellationToken);
//                }

//                // Get or create Seller
//                var seller = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == request.SellerPhoneNumber, cancellationToken);
//                if (seller == null)
//                {
//                    seller = new UserDetail
//                    {
//                        UserId = Guid.NewGuid().ToString(),
//                        PhoneNumber = request.SellerPhoneNumber,
//                        Created = DateTime.UtcNow,
//                        Role = nameof(Roles.User),
//                    };
//                    _context.UserDetails.Add(seller);
//                    await _context.SaveChangesAsync(cancellationToken);
//                }

//                // Determine FromID and ToID
//                int fromId, toId;
//                string? targetDeviceToken = null;

//                if (buyer.Id.ToString() == currentUserId)
//                {
//                    fromId = buyer.Id;
//                    toId = seller.Id;
//                    targetDeviceToken = seller.DeviceToken;
//                }
//                else if (seller.Id.ToString() == currentUserId)
//                {
//                    fromId = seller.Id;
//                    toId = buyer.Id;
//                    targetDeviceToken = buyer.DeviceToken;
//                }
//                else
//                {
//                    return Result<object>.Failure(StatusCodes.Status403Forbidden, "Unauthorized user context.");
//                }

//                // Create notification for user
//                var notification = new Notification
//                {
//                    FromID = fromId,
//                    ToID = toId,
//                    ContractId = request.ContractId,
//                    Type = request.Type,
//                    Title = request.Title,
//                    Description = request.Description,
//                    Created = DateTime.UtcNow,
//                    IsRead = false,
//                    GroupId = string.IsNullOrWhiteSpace(request.GroupId) ? null : request.GroupId,
//                };

//                _context.Notifications.Add(notification);
//                await _context.SaveChangesAsync(cancellationToken);

//                // Fetch contract title
//                var contract = await _context.ContractDetails
//                    .Where(c => c.Id == request.ContractId)
//                    .Select(c => new { c.Id, c.ContractTitle })
//                    .FirstOrDefaultAsync(cancellationToken);

//                var contractTitle = contract?.ContractTitle ?? "Contract";

//                var receiver = toId == buyer.Id ? buyer : seller;
//                var receiverName = receiver.FullName ?? "User";
//                var receiverImage = receiver.ProfilePicture ?? "";

//                // Send push to other user
//                if (!string.IsNullOrWhiteSpace(targetDeviceToken))
//                {
//                    try
//                    {
//                        await _notificationService.SendPushNotificationAsync(
//                            targetDeviceToken,
//                            request.Title,
//                            request.Description,
//                            new
//                            {
//                                GroupId = request.GroupId,
//                                ContractId = request.ContractId,
//                                ContractTitle = contractTitle,
//                                ReceiverName = receiverName,
//                                ReceiverImage = receiverImage,
//                                Type = request.Type
//                            }
//                        );
//                    }
//                    catch (Exception pushEx)
//                    {
//                        Console.WriteLine($"[Warning] Push notification failed: {pushEx.Message}");
//                    }
//                }

//                // Create and send notifications to all admins
//                var adminUsers = await _context.UserDetails
//                    .Where(u => u.Role == nameof(Roles.Admin))
//                    .ToListAsync(cancellationToken);

//                foreach (var admin in adminUsers)
//                {
//                    var adminNotification = new Notification
//                    {
//                        FromID = fromId,
//                        ToID = admin.Id,
//                        ContractId = request.ContractId,
//                        Type = request.Type,
//                        Title = request.Title,
//                        Description = request.Description,
//                        Created = DateTime.UtcNow,
//                        IsRead = false,
//                        GroupId = string.IsNullOrWhiteSpace(request.GroupId) ? null : request.GroupId,
//                    };

//                    _context.Notifications.Add(adminNotification);

//                    if (!string.IsNullOrWhiteSpace(admin.DeviceToken))
//                    {
//                        try
//                        {
//                            await _notificationService.SendPushNotificationAsync(
//                                admin.DeviceToken,
//                                $"[ADMIN] {request.Title}",
//                                request.Description,
//                                new
//                                {
//                                    GroupId = request.GroupId,
//                                    ContractId = request.ContractId,
//                                    ContractTitle = contractTitle,
//                                    FromUser = currentUserId,
//                                    Type = request.Type
//                                }
//                            );
//                        }
//                        catch (Exception adminPushEx)
//                        {
//                            Console.WriteLine($"[Warning] Admin push failed for {admin.Id}: {adminPushEx.Message}");
//                        }
//                    }
//                }

//                await _context.SaveChangesAsync(cancellationToken);

//                return Result<object>.Success(
//                    StatusCodes.Status200OK,
//                    "Notification created successfully (including admin).",
//                    new { NotificationId = notification.Id }
//                );
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[Error] CreateNotificationCommandHandler: {ex.Message}");
//                return Result<object>.Failure(StatusCodes.Status500InternalServerError, "Unexpected Server Error.");
//            }
//        }



//        //public async Task<Result<object>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
//        //{
//        //    try
//        //    {
//        //        if (string.IsNullOrWhiteSpace(request.BuyerPhoneNumber) || string.IsNullOrWhiteSpace(request.SellerPhoneNumber))
//        //            return Result<object>.Failure(StatusCodes.Status400BadRequest, "Phone numbers are required.");

//        //        if (request.ContractId <= 0)
//        //            return Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid ContractId.");

//        //        var currentUserId = _jwtService.GetUserId();

//        //        // Get or create Buyer
//        //        var buyer = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == request.BuyerPhoneNumber, cancellationToken);
//        //        if (buyer == null)
//        //        {
//        //            buyer = new UserDetail
//        //            {
//        //                UserId = Guid.NewGuid().ToString(),
//        //                PhoneNumber = request.BuyerPhoneNumber,
//        //                Created = DateTime.UtcNow,
//        //                Role = nameof(Roles.User),
//        //            };
//        //            _context.UserDetails.Add(buyer);
//        //            await _context.SaveChangesAsync(cancellationToken);
//        //        }

//        //        // Get or create Seller
//        //        var seller = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == request.SellerPhoneNumber, cancellationToken);
//        //        if (seller == null)
//        //        {
//        //            seller = new UserDetail
//        //            {
//        //                UserId = Guid.NewGuid().ToString(),
//        //                PhoneNumber = request.SellerPhoneNumber,
//        //                Created = DateTime.UtcNow,
//        //                Role = nameof(Roles.User)
//        //            };
//        //            _context.UserDetails.Add(seller);
//        //            await _context.SaveChangesAsync(cancellationToken);
//        //        }

//        //        // Determine FromID and ToID based on who is the logged-in user
//        //        int fromId, toId;
//        //        string? targetDeviceToken = null;

//        //        if (buyer.Id.ToString() == currentUserId)
//        //        {
//        //            fromId = buyer.Id;
//        //            toId = seller.Id;
//        //            targetDeviceToken = seller.DeviceToken;
//        //        }
//        //        else if (seller.Id.ToString() == currentUserId)
//        //        {
//        //            fromId = seller.Id;
//        //            toId = buyer.Id;
//        //            targetDeviceToken = buyer.DeviceToken;
//        //        }
//        //        else
//        //        {
//        //            return Result<object>.Failure(StatusCodes.Status403Forbidden, "Unauthorized user context.");
//        //        }

//        //        // Create notification record
//        //        var notification = new Notification
//        //        {
//        //            FromID = fromId,
//        //            ToID = toId,
//        //            ContractId = request.ContractId,
//        //            Type = request.Type,
//        //            Title = request.Title,
//        //            Description = request.Description,
//        //            Created = DateTime.UtcNow,
//        //            IsRead = false,
//        //            GroupId = string.IsNullOrWhiteSpace(request.GroupId) ? null : request.GroupId,
//        //        };

//        //        _context.Notifications.Add(notification);
//        //        await _context.SaveChangesAsync(cancellationToken);

//        //        // Fetch extra info for push payload
//        //        var contract = await _context.ContractDetails
//        //            .Where(c => c.Id == request.ContractId)
//        //            .Select(c => new { c.Id, c.ContractTitle })
//        //            .FirstOrDefaultAsync(cancellationToken);

//        //        var contractTitle = contract?.ContractTitle ?? "Contract";

//        //        var receiver = toId == buyer.Id ? buyer : seller;
//        //        var receiverName = receiver.FullName ?? "User";
//        //        var receiverImage = receiver.ProfilePicture ?? "";

//        //        // Send Push Notification
//        //        if (!string.IsNullOrWhiteSpace(targetDeviceToken))
//        //        {
//        //            try
//        //            {
//        //                await _notificationService.SendPushNotificationAsync(
//        //                    targetDeviceToken,
//        //                    request.Title,
//        //                    request.Description,
//        //                    new
//        //                    {
//        //                        GroupId = request.GroupId,
//        //                        ContractId = request.ContractId,
//        //                        ContractTitle = contractTitle,
//        //                        ReceiverName = receiverName,
//        //                        ReceiverImage = receiverImage,
//        //                        Type = request.Type
//        //                    }
//        //                );
//        //            }
//        //            catch (Exception pushEx)
//        //            {
//        //                Console.WriteLine($"[Warning] Push notification failed: {pushEx.Message}");
//        //            }
//        //        }

//        //        return Result<object>.Success(
//        //            StatusCodes.Status200OK,
//        //            "Notification created successfully.",
//        //            new { NotificationId = notification.Id }
//        //        );
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Console.WriteLine($"[Error] CreateNotificationCommandHandler: {ex.Message}");
//        //        return Result<object>.Failure(StatusCodes.Status500InternalServerError, "Unexpected Server Error.");
//        //    }
//        //}

//        //public async Task<Result<object>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
//        //{
//        //    try
//        //    {
//        //        if (string.IsNullOrWhiteSpace(request.BuyerPhoneNumber) || string.IsNullOrWhiteSpace(request.SellerPhoneNumber))
//        //            return Result<object>.Failure(StatusCodes.Status400BadRequest, "Phone numbers are required.");

//        //        if (request.ContractId <= 0)
//        //            return Result<object>.Failure(StatusCodes.Status400BadRequest, "Invalid ContractId.");

//        //        var currentUserId = _jwtService.GetUserId();

//        //        // Get or create Buyer
//        //        var buyer = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == request.BuyerPhoneNumber, cancellationToken);
//        //        if (buyer == null)
//        //        {
//        //            buyer = new UserDetail
//        //            {
//        //                UserId = Guid.NewGuid().ToString(),
//        //                PhoneNumber = request.BuyerPhoneNumber,
//        //                Created = DateTime.UtcNow,
//        //                Role = nameof(Roles.User),
//        //            };
//        //            _context.UserDetails.Add(buyer);
//        //            await _context.SaveChangesAsync(cancellationToken);
//        //        }

//        //        // Get or create Seller
//        //        var seller = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == request.SellerPhoneNumber, cancellationToken);
//        //        if (seller == null)
//        //        {
//        //            seller = new UserDetail
//        //            {
//        //                UserId = Guid.NewGuid().ToString(),
//        //                PhoneNumber = request.SellerPhoneNumber,
//        //                Created = DateTime.UtcNow,
//        //                Role = nameof(Roles.User)
//        //            };
//        //            _context.UserDetails.Add(seller);
//        //            await _context.SaveChangesAsync(cancellationToken);
//        //        }

//        //        // Determine FromID and ToID based on who is the logged-in user
//        //        int fromId, toId;
//        //        string? targetDeviceToken = null;

//        //        if (buyer.Id.ToString() == currentUserId)
//        //        {
//        //            fromId = buyer.Id;
//        //            toId = seller.Id;
//        //            targetDeviceToken = seller.DeviceToken;
//        //        }
//        //        else if (seller.Id.ToString() == currentUserId)
//        //        {
//        //            fromId = seller.Id;
//        //            toId = buyer.Id;
//        //            targetDeviceToken = buyer.DeviceToken;
//        //        }
//        //        else
//        //        {
//        //            return Result<object>.Failure(StatusCodes.Status403Forbidden, "Unauthorized user context.");
//        //        }

//        //        // Create notification record
//        //        var notification = new Notification
//        //        {
//        //            FromID = fromId,
//        //            ToID = toId,
//        //            ContractId = request.ContractId,
//        //            Type = request.Type,
//        //            Title = request.Title,
//        //            Description = request.Description,
//        //            Created = DateTime.UtcNow,
//        //            IsRead = false,
//        //            GroupId = string.IsNullOrWhiteSpace(request.GroupId) ? null : request.GroupId,
//        //        };

//        //        _context.Notifications.Add(notification);
//        //        await _context.SaveChangesAsync(cancellationToken);

//        //        // Send Push Notification
//        //        if (!string.IsNullOrWhiteSpace(targetDeviceToken))
//        //        {
//        //            try
//        //            {
//        //                await _notificationService.SendPushNotificationAsync(
//        //                    targetDeviceToken,
//        //                    request.Title,
//        //                request.Description,
//        //                    new { ContractId = request.ContractId, Type = request.Type, GroupId = request.GroupId, }
//        //                );
//        //            }
//        //            catch (Exception pushEx)
//        //            {
//        //                Console.WriteLine($"[Warning] Push notification failed: {pushEx.Message}");
//        //            }
//        //        }

//        //        return Result<object>.Success(
//        //            StatusCodes.Status200OK,
//        //            "Notification created successfully.",
//        //            new { NotificationId = notification.Id }
//        //        );
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Console.WriteLine($"[Error] CreateNotificationCommandHandler: {ex.Message}");
//        //        return Result<object>.Failure(StatusCodes.Status500InternalServerError, "Unexpected Server Error.");
//        //    }
//        //}
//    }
//}
