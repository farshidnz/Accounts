using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Domain.Entities.ReportSubscription
{
    public class ShopGoPerson
    {
        public int PersonId { get; set; }
        public CognitoId CognitoId { get; set; }
        public int PremiumStatus { get; set; }
        public int TrustScore { get; set; }
#nullable enable
        public string? OriginationSource { get; set; }
#nullable disable
        public DateTime CreatedDateUTC { get; set; }
        public DateTime? UpdatedDateUTC { get; set; }
    }

    public class CognitoId
    {
        public string Guid { get; set; }
    }

}
