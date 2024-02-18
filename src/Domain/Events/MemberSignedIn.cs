using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Domain.Events
{
    public class MemberSignedIn : AccountsEventBase
    {
        // by default, all timestamp are UTC format
        public DateTime LastLogonUtc { get; set; } 
    }
}
