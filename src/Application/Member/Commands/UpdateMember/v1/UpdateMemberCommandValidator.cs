using FluentValidation;

namespace Accounts.Application.Member.Commands.UpdateMember.v1;

public class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(r => r.CognitoId).NotEmpty();
        RuleFor(r => r.PostCode).MaximumLength(4);
        RuleFor(r => r.Mobile).MaximumLength(15);
        RuleFor(r => r.Email).EmailAddress();
    }
}