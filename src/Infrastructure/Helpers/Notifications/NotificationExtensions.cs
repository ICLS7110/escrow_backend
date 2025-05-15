using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Api.Infrastructure.Helpers.Notifications;
public static class NotificationExtensions
{
    public static Dictionary<string, string> ToDictionary(this object obj)
    {
        if (obj == null) return new Dictionary<string, string>();

        return obj.GetType()
                  .GetProperties()
                  .Where(p => p.GetValue(obj) != null)
                  .ToDictionary(
                      p => p.Name,
                      p => p.GetValue(obj)?.ToString() ?? string.Empty
                  );
    }
}
