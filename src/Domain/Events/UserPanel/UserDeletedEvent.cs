namespace Escrow.Api.Domain.Events.UserPanel;
public class UserDeletedEvent : BaseEvent
{
    public User User { get; }
    public UserDeletedEvent(User user)
    {
        User = user;
    }
}
