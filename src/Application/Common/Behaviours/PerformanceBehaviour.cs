using Microsoft.Extensions.Logging;
using Accounts.Application.Common.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;
using MassTransit;

namespace Accounts.Application.Common.Behaviours
{
    public class PerformanceBehaviour<TMessage> :
                    IFilter<ConsumeContext<TMessage>>
                    where TMessage : class
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<TMessage> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IIdentityService _identityService;

        public virtual long ElapsedMilliseconds => _timer.ElapsedMilliseconds;

        public PerformanceBehaviour(
            ILogger<TMessage> logger,
            ICurrentUserService currentUserService,
            IIdentityService identityService)
        {
            _timer = new Stopwatch();

            _logger = logger;
            _currentUserService = currentUserService;
            _identityService = identityService;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("performance-logging-filter");
        }

        public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            _timer.Start();

            await next.Send(context);

            _timer.Stop();

            if (IsSlowProcessingMessage())
            {
                var requestName = typeof(TMessage).Name;
                var userId = _currentUserService.UserId ?? string.Empty;
                var userName = string.Empty;

                if (!string.IsNullOrEmpty(userId))
                {
                    userName = await _identityService.GetUserNameAsync(userId);
                }

                _logger.LogWarning("Accounts Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                                                                            requestName, ElapsedMilliseconds, userId, userName, context.Message);
            }
        }

        private bool IsSlowProcessingMessage()
        {
            return ElapsedMilliseconds > 500;
        }
    }
}