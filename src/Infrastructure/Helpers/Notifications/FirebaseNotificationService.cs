

using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.Enums;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Escrow.Api.Infrastructure.Helpers.Notifications
{
    public class FirebaseNotificationService : INotificationService
    {
        private static bool _isInitialized = false;
        private readonly ILogger<FirebaseNotificationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IApplicationDbContext _context;

        public FirebaseNotificationService(
            IConfiguration configuration,
            ILogger<FirebaseNotificationService> logger,
            IApplicationDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;

            if (!_isInitialized)
            {
                try
                {
                    var path = _configuration["Firebase:CredentialsPath"];
                    if (string.IsNullOrEmpty(path))
                    {
                        throw new InvalidOperationException("Firebase credentials path is not configured.");
                    }

                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(path)
                    });

                    _isInitialized = true;
                    _logger.LogInformation("FirebaseApp initialized successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error initializing FirebaseApp.");
                    throw;
                }
            }
        }

        public async Task<bool> SendPushNotificationAsync(string deviceToken, string title, string body, object? data = null)
        {
            try
            {
                var message = new Message
                {
                    Token = deviceToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data?.ToDictionary() ?? new Dictionary<string, string>()
                };

                var result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("Push notification sent. Result: {Result}", result);

                return true;
            }
            catch (FirebaseMessagingException fcmEx)
            {
                _logger.LogError(fcmEx, "Firebase messaging error: {Error}", fcmEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending push notification.");
                return false;
            }
        }

        public async Task SendNotificationAsync(int creatorId, int buyerId, int sellerId, int contractId, string role, CancellationToken cancellationToken)
        {
            var creatorName = await _context.UserDetails
                .Where(u => u.Id == creatorId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(cancellationToken);

            var users = await _context.UserDetails
                .Where(u => u.Id == buyerId || u.Id == sellerId)
                .ToListAsync(cancellationToken);

            var userToNotify = users.FirstOrDefault(u => u.Id != creatorId);

            if (userToNotify != null)
            {
                var userInfo = new { userToNotify.FullName, userToNotify.DeviceToken, userToNotify.IsNotified };

                var title = "New Contract Created";
                var description = $"{creatorName} has created a new contract for you, {userInfo.FullName}. Please review the details.";

                var notification = new Domain.Entities.Notifications.Notification
                {
                    ToID = userToNotify.Id,
                    ContractId = contractId,
                    Title = title,
                    Description = description,
                    Type = "Contract",
                    IsRead = false,
                    Created = DateTime.UtcNow,
                };

                await _context.Notifications.AddAsync(notification, cancellationToken);

                if (!string.IsNullOrEmpty(userInfo.DeviceToken) && userInfo.IsNotified == true)
                {
                    await SendPushNotificationAsync(
                        userInfo.DeviceToken,
                        title,
                        description,
                        new { ContractId = contractId, Type = "Contract", Role = role }
                    );
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> GetOrCreateUserId(string? name, string? mobile, CancellationToken cancellationToken)
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

    internal static class ObjectExtensions
    {
        public static Dictionary<string, string> ToDictionaryFromObject(this object obj)
        {
            return obj.GetType().GetProperties()
                .Where(p => p.GetValue(obj) != null)
                .ToDictionary(p => p.Name, p => p.GetValue(obj)?.ToString() ?? "");
        }
    }

}
