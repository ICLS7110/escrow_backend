
using Escrow.Api.Application.ContractPanel;
using Escrow.Api.Application.DTOs;


namespace Escrow.Api.Web.Endpoints.ContractPanel;

public class Contract : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var userGroup = app.MapGroup(this)
        .RequireAuthorization() // Enable OpenIddict authorization
        .WithOpenApi()
        .AddEndpointFilter(async (context, next) =>
        {
            // Optional: Add custom authorization logic if needed
            return await next(context);
        });
        userGroup.MapPost("/", CreateContractDetails).RequireAuthorization(p => p.RequireRole("User"));
    }

    public async Task<IResult> CreateContractDetails(ISender sender, CreateContractDetailCommand command)
    {
        var id = await sender.Send(command);
        
        return TypedResults.Ok(Result<object>.Success(StatusCodes.Status204NoContent, "Contract Created Successfully.",new()));
    }
}
