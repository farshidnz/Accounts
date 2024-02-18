using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Domain.Events
{
    // The PIIData should inclde the encryped email
    public class CognitoLinked : AccountsEventBase
    {
        public string CognitoPoolId { get; set; }
    }
}
