using Accounts.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MassTransit;

namespace Accounts.Application.Common.Behaviours
{
    public class LoggingBehaviour<TMessage> :
                    IFilter<ConsumeContext<TMessage>>
                    where TMessage : class
    {
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IIdentityService _identityService;

        public LoggingBehaviour(ILogger<TMessage> logger, ICurrentUserService currentUserService, IIdentityService identityService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
            _identityService = identityService;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("logging-filter");
        }

        public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            var requestName = typeof(TMessage).Name;

            var userId = _currentUserService.UserId ?? string.Empty;

            string userName = !string.IsNullOrWhiteSpace(userId) ? await _identityService.GetUserNameAsync(userId) : string.Empty;

            _logger.LogInformation("Accounts Request: {Name} {@UserId} {@UserName} {@Request}",
                                                                        requestName, userId, userName, context.Message);

            await next.Send(context);
        }
    }
}