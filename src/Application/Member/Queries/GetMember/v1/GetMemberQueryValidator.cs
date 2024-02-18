namespace Accounts.Application.Member.Queries.GetMember.v1;

using FluentValidation;

public class GetMemberQueryValidator : AbstractValidator<GetMemberQuery>
{
    public GetMemberQueryValidator()
    {
        RuleFor(v => v).NotEmpty().WithMessage("Please enter either a CognitoId or Email address");
    }
}