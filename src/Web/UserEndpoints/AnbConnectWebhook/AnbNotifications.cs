using System.Text;
using System.Text.Json;
using Escrow.Api.Application.AnbConnectWebhook.Commands;

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

    public async Task<IResult> HandleWebhookNotification(HttpRequest request,
        ISender sender)
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

        var command = JsonSerializer.Deserialize<WebhookNotificationCommand>(jsonInput,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new WebhookNotificationCommand();

        await sender.Send(command);
        return Results.Ok();
    }
}
