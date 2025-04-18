using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Application.Common.Interfaces;
public interface INotificationService
{
    Task<bool> SendPushNotificationAsync(string deviceToken, string title, string body, object? data = null);
}
