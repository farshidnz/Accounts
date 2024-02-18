using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Domain.Entities.ReportSubscription
{
    public class ShopGoCognitoMember
    {
        public int MappingId { get; set; }
        public string CognitoId { get; set; }
        public int MemberId { get; set; }
        public string CognitopoolId { get; set; }
        public MemberNewId MemberNewId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Status { get; set; }
        public int PersonId { get; set; }
    }
}
