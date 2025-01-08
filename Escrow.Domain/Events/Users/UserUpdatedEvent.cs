using Escrow.Domain.Common;
using Escrow.Domain.Entities.UserPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Domain.Events.Users
{
    internal class UserUpdatedEvent : BaseEvent
    {
        public User User { get; }
        public UserUpdatedEvent(User user)
        {
            User = user;
        }
    }
}
