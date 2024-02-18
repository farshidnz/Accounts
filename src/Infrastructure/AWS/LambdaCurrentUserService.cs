using Accounts.Application.Common.Interfaces;

namespace Accounts.Infrastructure.AWS
{
    public class LambdaCurrentUserService : ICurrentUserService
    {
        // TODO: what to do? what to do?
        public string UserId => "LambdaUser";
    }
}