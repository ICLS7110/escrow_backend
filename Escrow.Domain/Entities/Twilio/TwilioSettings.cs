using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escrow.Domain.Entities.Twilio
{
    public class TwilioSettings
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string PhoneNumber { get; set; }
    }
}
