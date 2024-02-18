using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Domain.Events
{
    // The PIIData should include encryped email or encyrped phone
    public class MemberCredentialChanged : AccountsEventBase
    {
    }
}
