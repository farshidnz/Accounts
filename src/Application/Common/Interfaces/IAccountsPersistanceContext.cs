using System.Threading.Tasks;

namespace Accounts.Application.Common.Interfaces;

using Accounts.Domain.Common;
using Domain.Entities;

public interface IAccountsPersistanceContext<Key, Entity> where Entity : IHasIdentity<Key>
{
    Task<Member> GetMember(MemberContext context);
    Task<MemberCognito> GetCognitoPoolId(string cognitoPoolName);
    Task AddOrUpdate(Entity context);
}