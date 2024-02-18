namespace Accounts.Application.Member.Queries.GetMember.v1;

public class GetMemberQuery
{
    public string Email { get; set; }

    public System.Guid? CognitoId { get; set; }
}