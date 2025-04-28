using System.Text.Json;
using Escrow.Api.Domain.Entities.AnbConnectWebhook;

namespace Escrow.Api.Application.AnbConnectWebhook.Commands;
public class ProcessStatementCreatedCommand : INotificationCommand
{
    public string EventType => "statement.created";
    public JsonElement RawPayload { get; init; }
    public StatementCreatedPayload Payload => JsonSerializer.Deserialize<StatementCreatedPayload>(RawPayload.GetRawText(), NotificationPayloadFactory.SerializerOptions)
        ?? throw new JsonException($"Failed to deserialize payload for {EventType}. Deserialization returned null.");
}

public class ProcessPaymentReceivedCommand : INotificationCommand
{
    public string EventType => "payment.received";
    public JsonElement RawPayload { get; init; }
    public PaymentReceivedPayload Payload => JsonSerializer.Deserialize<PaymentReceivedPayload>(RawPayload.GetRawText(), NotificationPayloadFactory.SerializerOptions)
        ?? throw new JsonException($"Failed to deserialize payload for {EventType}. Deserialization returned null.");
}

public class ProcessPaymentCompletedCommand : INotificationCommand
{
    public string EventType => "payment.completed";
    public JsonElement RawPayload { get; init; }
    public PaymentCompletedPayload Payload => JsonSerializer.Deserialize<PaymentCompletedPayload>(RawPayload.GetRawText(), NotificationPayloadFactory.SerializerOptions)
        ?? throw new JsonException($"Failed to deserialize payload for {EventType}. Deserialization returned null.");
}


public class ProcessWpsGeneratedCommand : INotificationCommand
{
    public string EventType => "wps.generated";
    public JsonElement RawPayload { get; init; }
    public WpsGeneratedPayload Payload => JsonSerializer.Deserialize<WpsGeneratedPayload>(RawPayload.GetRawText(), NotificationPayloadFactory.SerializerOptions)
        ?? throw new JsonException($"Failed to deserialize payload for {EventType}. Deserialization returned null.");
}

public class ProcessWpsPaymentSuccessCommand : INotificationCommand
{
    public string EventType => "wps.payment.success";
    public JsonElement RawPayload { get; init; }
    public WpsPaymentSuccessPayload Payload => JsonSerializer.Deserialize<WpsPaymentSuccessPayload>(RawPayload.GetRawText(), NotificationPayloadFactory.SerializerOptions)
        ?? throw new JsonException($"Failed to deserialize payload for {EventType}. Deserialization returned null.");
}

public class ProcessWpsSentCommand : INotificationCommand
{
    public string EventType => "wps.sent";
    public JsonElement RawPayload { get; init; }
    public WpsSentPayload Payload => JsonSerializer.Deserialize<WpsSentPayload>(RawPayload.GetRawText(), NotificationPayloadFactory.SerializerOptions)
        ?? throw new JsonException($"Failed to deserialize payload for {EventType}. Deserialization returned null.");
}

public class ProcessWpsInsufficientFundsCommand : INotificationCommand
{
    public string EventType => "wps.insufficient.funds";
    public JsonElement RawPayload { get; init; }
    public WpsInsufficientFundsPayload Payload => JsonSerializer.Deserialize<WpsInsufficientFundsPayload>(RawPayload.GetRawText(), NotificationPayloadFactory.SerializerOptions)
        ?? throw new JsonException($"Failed to deserialize payload for {EventType}. Deserialization returned null.");
}

public class ProcessWpsSuccessCommand : INotificationCommand
{
    public string EventType => "wps.success";
    public JsonElement RawPayload { get; init; }
    public WpsSuccessPayload Payload => JsonSerializer.Deserialize<WpsSuccessPayload>(RawPayload.GetRawText(), NotificationPayloadFactory.SerializerOptions)
        ?? throw new JsonException($"Failed to deserialize payload for {EventType}. Deserialization returned null.");
}

public class ProcessWpsFailedCommand : INotificationCommand
{
    public string EventType => "wps.failed";
    public JsonElement RawPayload { get; init; }
    public WpsFailedPayload Payload => JsonSerializer.Deserialize<WpsFailedPayload>(RawPayload.GetRawText(), NotificationPayloadFactory.SerializerOptions)
        ?? throw new JsonException($"Failed to deserialize payload for {EventType}. Deserialization returned null.");
}

public class ProcessPaymentStatusUpdatedCommand : INotificationCommand
{
    public string EventType => "payment.status.updated";
    public JsonElement RawPayload { get; init; }
    public PaymentStatusUpdatedPayload Payload => JsonSerializer.Deserialize<PaymentStatusUpdatedPayload>(RawPayload.GetRawText(), NotificationPayloadFactory.SerializerOptions)
        ?? throw new JsonException($"Failed to deserialize payload for {EventType}. Deserialization returned null.");
}
public static class NotificationCommandFactory
{
    public static INotificationCommand CreateCommand(string eventType, JsonElement payloadElement)
    {
        return eventType switch
        {
            "statement.created" => new ProcessStatementCreatedCommand { RawPayload = payloadElement },
            "payment.received" => new ProcessPaymentReceivedCommand { RawPayload = payloadElement },
            "payment.completed" => new ProcessPaymentCompletedCommand { RawPayload = payloadElement },
            "wps.generated" => new ProcessWpsGeneratedCommand { RawPayload = payloadElement },
            "wps.payment.success" => new ProcessWpsPaymentSuccessCommand { RawPayload = payloadElement },
            "wps.sent" => new ProcessWpsSentCommand { RawPayload = payloadElement },
            "wps.insufficient.funds" => new ProcessWpsInsufficientFundsCommand { RawPayload = payloadElement },
            "wps.success" => new ProcessWpsSuccessCommand { RawPayload = payloadElement },
            "wps.failed" => new ProcessWpsFailedCommand { RawPayload = payloadElement },
            "payment.status.updated" => new ProcessPaymentStatusUpdatedCommand { RawPayload = payloadElement },
            _ => throw new ArgumentException($"Unknown event type for command creation: {eventType}", nameof(eventType))
        };
    }
}
public static class NotificationPayloadFactory
{
    public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
}
