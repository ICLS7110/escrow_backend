using System;
using System.Threading;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.Common.Models;
using Escrow.Api.Application.DTOs;
using Escrow.Api.Domain.Entities.Notifications;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Escrow.Api.Application.Notifications.Commands
{
    public class CreateNotificationCommand : IRequest<Result<int>>
    {
        public string BuyerPhoneNumber { get; set; } = string.Empty;
        public string SellerPhoneNumber { get; set; } = string.Empty;
        public int ContractId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IJwtService _jwtService;

        public CreateNotificationCommandHandler(
            IApplicationDbContext context,
            INotificationService notificationService,
            IJwtService jwtService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public async Task<Result<int>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.BuyerPhoneNumber) || string.IsNullOrWhiteSpace(request.SellerPhoneNumber))
                    return Result<int>.Failure(StatusCodes.Status400BadRequest, "Phone numbers are required.");

                if (request.ContractId <= 0)
                    return Result<int>.Failure(StatusCodes.Status400BadRequest, "Invalid ContractId.");

                var currentUserId = _jwtService.GetUserId();

                // Get or create Buyer
                var buyer = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == request.BuyerPhoneNumber, cancellationToken);
                if (buyer == null)
                {
                    buyer = new UserDetail
                    {
                        UserId = Guid.NewGuid().ToString(),
                        PhoneNumber = request.BuyerPhoneNumber,
                        Created = DateTime.UtcNow,
                        Role= nameof(Roles.User)
                    };
                    _context.UserDetails.Add(buyer);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                // Get or create Seller
                var seller = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == request.SellerPhoneNumber, cancellationToken);
                if (seller == null)
                {
                    seller = new UserDetail
                    {
                        UserId = Guid.NewGuid().ToString(),
                        PhoneNumber = request.SellerPhoneNumber,
                        Created = DateTime.UtcNow,
                        Role = nameof(Roles.User)
                    };
                    _context.UserDetails.Add(seller);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                // Determine FromID and ToID based on who is the logged-in user
                int fromId, toId;
                string? targetDeviceToken = null;

                if (buyer.UserId == currentUserId)
                {
                    fromId = buyer.Id;
                    toId = seller.Id;
                    targetDeviceToken = seller.DeviceToken;
                }
                else if (seller.UserId == currentUserId)
                {
                    fromId = seller.Id;
                    toId = buyer.Id;
                    targetDeviceToken = buyer.DeviceToken;
                }
                else
                {
                    return Result<int>.Failure(StatusCodes.Status403Forbidden, "Unauthorized user context.");
                }

                // Create notification record
                var notification = new Notification
                {
                    FromID = fromId,
                    ToID = toId,
                    ContractId = request.ContractId,
                    Type = request.Type,
                    Title = request.Title,
                    Description = request.Description,
                    Created = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync(cancellationToken);

                // Send Push Notification to ToID
                if (!string.IsNullOrWhiteSpace(targetDeviceToken))
                {
                    try
                    {
                        await _notificationService.SendPushNotificationAsync(
                            targetDeviceToken,
                            request.Title,
                            request.Description,
                            new { ContractId = request.ContractId, Type = request.Type }
                        );
                    }
                    catch (Exception pushEx)
                    {
                        Console.WriteLine($"[Warning] Push notification failed: {pushEx.Message}");
                    }
                }

                return Result<int>.Success(StatusCodes.Status200OK, "Notification created and push sent (if available).", notification.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] CreateNotificationCommandHandler: {ex.Message}");
                return Result<int>.Failure(StatusCodes.Status500InternalServerError, "Unexpected Server Error.");
            }
        }
    }
}


































//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using Escrow.Api.Application.Common.Models;
//using Escrow.Api.Application.DTOs;
//using Escrow.Api.Domain.Entities;
//using Escrow.Api.Domain.Entities.Notifications;
//using Escrow.Api.Domain.Entities.UserPanel;
//using MediatR;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;

//namespace Escrow.Api.Application.Notifications.Commands
//{
//    public class CreateNotificationCommand : IRequest<Result<int>>
//    {
//        public string BuyerPhoneNumber { get; set; } = string.Empty;
//        public string SellerPhoneNumber { get; set; } = string.Empty;
//        public int ContractId { get; set; }
//        public string Type { get; set; } = string.Empty;
//        public string Title { get; set; } = string.Empty;
//        public string Description { get; set; } = string.Empty;
//    }

//    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<int>>
//    {
//        private readonly IApplicationDbContext _context;

//        public CreateNotificationCommandHandler(IApplicationDbContext context)
//        {
//            _context = context ?? throw new ArgumentNullException(nameof(context));
//        }

//        public async Task<Result<int>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
//        {
//            try
//            {
//                Console.WriteLine($"Received CreateNotificationCommand: Title={request.Title}, Buyer={request.BuyerPhoneNumber}, Seller={request.SellerPhoneNumber}");

//                // Validate input
//                if (string.IsNullOrWhiteSpace(request.BuyerPhoneNumber) || string.IsNullOrWhiteSpace(request.SellerPhoneNumber))
//                {
//                    return Result<int>.Failure(StatusCodes.Status400BadRequest, "Phone numbers are required.");
//                }

//                if (request.ContractId <= 0)
//                {
//                    return Result<int>.Failure(StatusCodes.Status400BadRequest, "Invalid ContractId.");
//                }

//                // Retrieve or create buyer
//                var buyer = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == request.BuyerPhoneNumber, cancellationToken);
//                if (buyer == null)
//                {
//                    buyer = new UserDetail { UserId = Guid.NewGuid().ToString(), PhoneNumber = request.BuyerPhoneNumber, Created = DateTime.UtcNow };
//                    _context.UserDetails.Add(buyer);
//                    await _context.SaveChangesAsync(cancellationToken);
//                }

//                // Retrieve or create seller
//                var seller = await _context.UserDetails.FirstOrDefaultAsync(u => u.PhoneNumber == request.SellerPhoneNumber, cancellationToken);
//                if (seller == null)
//                {
//                    seller = new UserDetail { UserId = Guid.NewGuid().ToString(), PhoneNumber = request.SellerPhoneNumber, Created = DateTime.UtcNow };
//                    _context.UserDetails.Add(seller);
//                    await _context.SaveChangesAsync(cancellationToken);
//                }

//                // Create Notification
//                var notification = new Notification
//                {
//                    FromID = seller.Id,
//                    ToID = buyer.Id,
//                    ContractId = request.ContractId,
//                    Type = request.Type,
//                    Title = request.Title,
//                    Description = request.Description,
//                    Created = DateTime.UtcNow
//                };

//                _context.Notifications.Add(notification);
//                await _context.SaveChangesAsync(cancellationToken);

//                Console.WriteLine($"Notification created successfully with ID: {notification.Id}");
//                return Result<int>.Success(StatusCodes.Status200OK, "Notification created successfully.", notification.Id);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error in CreateNotificationCommandHandler: {ex.Message}");
//                return Result<int>.Failure(StatusCodes.Status500InternalServerError, "Unexpected Server Error.");
//            }
//        }
//    }
//}
