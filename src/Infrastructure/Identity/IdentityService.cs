using Accounts.Application.Common.Interfaces;
using Accounts.Application.Common.Models;
using System.Threading.Tasks;

namespace Accounts.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        public Task<string> GetUserNameAsync(string userId)
        {
            // TODO: is implementation of this function required, if so should this come from claim?
            return Task.FromResult(string.Empty);
        }

        public Task<bool> IsInRoleAsync(string userId, string role)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> AuthorizeAsync(string userId, string policyName)
        {
            throw new System.NotImplementedException();
        }

        public Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Result> DeleteUserAsync(string userId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Result> DeleteUserAsync(ApplicationUser user)
        {
            throw new System.NotImplementedException();
        }
    }
}