using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Accounts.Application.Common.Behaviours
{
    public class UnhandledExceptionBehaviour<TMessage> :
                    IFilter<ConsumeContext<TMessage>>
                    where TMessage : class
    {
        private readonly ILogger<TMessage> _logger;

        public UnhandledExceptionBehaviour(ILogger<TMessage> logger)
        {
            _logger = logger;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("unhandled-exception-logging-filter");
        }

        public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            try
            {
                await next.Send(context);
            }
            catch (Exception ex)
            {
                var requestName = typeof(TMessage).Name;

                _logger.LogError(ex, "Accounts Request: Unhandled Exception for Request {Name} {@Request}", requestName, context.Message);

                throw;
            }
        }
    }
}