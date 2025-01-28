namespace Escrow.Api.Web.Infrastructure
{
    public abstract class EndpointGroupBase
    {
        public abstract void Map(WebApplication app);
    }

    // Derived class to map controllers
    public class ControllerEndpointGroup : EndpointGroupBase
    {
        public override void Map(WebApplication app)
        {
            // This maps all the controllers in the app
            app.MapControllers();  // Maps all controller actions to endpoints
        }
    }
}
