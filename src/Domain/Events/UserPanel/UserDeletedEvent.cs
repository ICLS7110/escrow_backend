using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Domain.Common;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Domain.Events.UserPanel;
public class UserDeletedEvent : BaseEvent
{
    public UserDetail User { get; }
    public UserDeletedEvent(UserDetail user)
    {
        User = user;
    }
}
