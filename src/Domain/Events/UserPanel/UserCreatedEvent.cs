namespace Escrow.Api.Domain.Events.UserPanel;
public class UserCreatedEvent : BaseEvent
{
    public User User { get; }
    public UserCreatedEvent(User user)
    {
        User = user;
    }
}
