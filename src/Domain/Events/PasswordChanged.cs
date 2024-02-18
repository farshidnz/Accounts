using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Domain.Events
{
    // The PIIData should inclde the encryped email
    public class PasswordChanged : AccountsEventBase
    {
        public string CognitoPoolId { get; set; }
        public int OriginationSource { get; set; }
    }
}
