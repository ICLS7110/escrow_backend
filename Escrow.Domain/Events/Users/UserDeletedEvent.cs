using Escrow.Domain.Common;
using Escrow.Domain.Entities.UserPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Domain.Events.Users
{
    public class UserDeletedEvent : BaseEvent
    {
        public User User { get; }
        public UserDeletedEvent(User user)
        {
            User = user;
        }
    }
}
