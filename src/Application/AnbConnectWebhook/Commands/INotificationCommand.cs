using System.Text.Json;

namespace Escrow.Api.Application.AnbConnectWebhook.Commands;
public interface INotificationCommand : IRequest<Unit>
{
    string EventType { get; }
    JsonElement RawPayload { get; }
}
