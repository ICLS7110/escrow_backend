using System.Text.Json;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Domain.Entities.AnbConnectWebhook;
using Microsoft.Extensions.Logging;

namespace Escrow.Api.Application.AnbConnectWebhook.Commands;
public class WebHookNotificationCreatedCommandHandler : IRequestHandler<WebhookNotificationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<WebHookNotificationCreatedCommandHandler> _logger;

    public WebHookNotificationCreatedCommandHandler(IApplicationDbContext context, ILogger<WebHookNotificationCreatedCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(WebhookNotificationCommand request, CancellationToken cancellationToken)
    {
        await SaveLog(request, cancellationToken);
        await Task.CompletedTask;
    }

    private async Task SaveLog(WebhookNotificationCommand request, CancellationToken cancellationToken)
    {
        var jsonPayload = JsonSerializer.Serialize(request.Payload);
        _logger.LogInformation("Webhook received at {ReceivedAt} | EventType: {EventType} | Payload: {Payload}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"), request.Type, request.Payload);
        AnbWebhookLog logEntry = new()
        {
            ReceivedAt = DateTime.UtcNow,
            EventType = request.Type ?? "",
            Payload = jsonPayload,
        };


        try
        {
            _context.AnbWebhookLogs.Add(logEntry);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Webhook saved to database [ID: {LogId}] at {ReceivedAt} | EventType: {EventType}", logEntry.Id, logEntry.ReceivedAt.ToString("yyyy-MM-dd HH:mm:ss.fff"), logEntry.EventType);
        }
        catch (Exception dbEx)
        {
            _logger.LogError(dbEx, "Failed to save webhook request log to database, EventType: {EventType}", request.Type);
        }
    }
}
