using Accounts.Domain.Common;
using System;
using System.Collections.Generic;

namespace Accounts.Domain.Entities
{
    public class CognitoId : ValueObject
    {
        private readonly Guid? _cognitoId;

        public CognitoId(Guid? cognitoId)
        {
            _cognitoId = cognitoId;
        }

      
        public override string ToString()
        {
            return _cognitoId.ToString();
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return _cognitoId;
        }
    }
}