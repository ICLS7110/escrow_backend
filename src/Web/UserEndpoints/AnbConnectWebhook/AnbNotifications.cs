using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Escrow.Api.Application.AnbConnectWebhook.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Escrow.Api.Web.UserEndpoints.AnbConnectWebhook;

public class AnbNotifications : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);


        group.MapPost("/webhook", HandleWebhookNotification)
             .WithName("HandleAnbNotification")
             .Produces(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest);
    }
    public async Task<IResult> HandleWebhookNotification(
            HttpRequest request,
            [FromServices] IMediator mediator)
    {
        string jsonInput;

        try
        {
            using (var reader = new StreamReader(request.Body, Encoding.UTF8))
            {
                jsonInput = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(jsonInput))
            {
                return Results.BadRequest("Request body cannot be empty.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading request body: {ex.Message}");
            return Results.BadRequest("Error reading request body.");
        }

        try
        {
            var notification = JsonSerializer.Deserialize<WebhookNotification>(jsonInput,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (notification == null || string.IsNullOrEmpty(notification.Type))
            {
                return Results.BadRequest("Invalid notification format or missing type.");
            }

            var command = NotificationCommandFactory.CreateCommand(notification.Type, notification.Payload);

            await mediator.Send(command);

            return Results.Ok();
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"Error deserializing notification JSON: {jsonEx.Message}");
            return Results.Ok();
        }
        catch (ArgumentException argEx)
        {
            Console.WriteLine($"Error processing notification: {argEx.Message}");
            return Results.Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return Results.Ok();
        }
    }

    public class WebhookNotification
    {
        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("payload")]
        public JsonElement Payload { get; set; }
    }
}
