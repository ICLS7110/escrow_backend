namespace Escrow.Api.Domain.Events.UserPanel;
public class UserUpdatedEvent : BaseEvent
{
    public User User { get; }
    public UserUpdatedEvent(User user)
    {
        User = user;
    }
}
