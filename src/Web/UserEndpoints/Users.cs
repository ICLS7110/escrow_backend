using Escrow.Api.Domain.Entities.Authentication;
using Escrow.Api.Infrastructure.Identity;

namespace Escrow.Api.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapIdentityApi<ApplicationUser1>();
    }
}
