using FluentValidation;
using System.Linq;

namespace Accounts.Application.Member.Commands.VerifyEmail;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(r => r.Code).NotNull().MinimumLength(10).Must(IsCodeValid).WithMessage("Invalid Code");
    }

    private static bool IsCodeValid(string code)
    {
        string[] decodeDataArr;
        string memberId, hashedEmail;
        bool valid = false;
        decodeDataArr = code.Split("$");
        if (decodeDataArr.Length == 2)
        {
            memberId = decodeDataArr[0];
            hashedEmail = string.Join("$", decodeDataArr.Skip(1).ToArray());

            if (!string.IsNullOrWhiteSpace(memberId) && !string.IsNullOrWhiteSpace(hashedEmail))
                valid = true;
        }
        return valid;
    }
}