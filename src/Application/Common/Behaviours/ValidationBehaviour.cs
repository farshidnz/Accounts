using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MassTransit;

using ValidationException = Accounts.Application.Common.Exceptions.ValidationException;

namespace Accounts.Application.Common.Behaviours
{
    public class ValidationBehaviour<TMessage> :
                    IFilter<ConsumeContext<TMessage>>
                    where TMessage : class
    {
        private readonly IEnumerable<IValidator<TMessage>> _validators;

        public ValidationBehaviour(IEnumerable<IValidator<TMessage>> validators)
        {
            _validators = validators;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("validation-filter");
        }

        public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            if (_validators.Any())
            {
                var validationContext = new ValidationContext<TMessage>(context.Message);

                var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(validationContext, context.CancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failures.Any())
                    throw new ValidationException(failures);
            }

            await next.Send(context);
        }
    }
}