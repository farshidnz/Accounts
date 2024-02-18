using FluentValidation;

namespace Accounts.Application.Member.Commands.CreateMember.v1;

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(r => r.CognitoId).NotEmpty();
        RuleFor(r => r.Email).NotEmpty().EmailAddress();
        RuleFor(r => r.ClientId).NotEmpty();
    }
}