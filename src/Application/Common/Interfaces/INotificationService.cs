using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Interfaces;
public interface INotificationService
{

    Task SendNotificationAsync(int creatorId, int buyerId, int sellerId, int contractId, string role,string type, CancellationToken cancellationToken);
    Task<int> GetOrCreateUserId(string? name, string? mobile, CancellationToken cancellationToken);

    Task<bool> SendPushNotificationAsync(string deviceToken, string title, string body, object? data = null);
}
