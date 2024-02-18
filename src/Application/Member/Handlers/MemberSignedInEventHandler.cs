using Accounts.Application.Common.Interfaces;
using Accounts.Application.Common.Models;
using Accounts.Application.Member.Commands.UpdateMember.v1;
using Accounts.Domain.Common;
using Accounts.Domain.Entities;
using Accounts.Domain.Events;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Accounts.Application.Member.Handlers
{
    public class MemberSignedInEventHandler : IConsumer<DomainEventNotification<MemberSignedIn>>
    {
        private readonly ILogger<MemberSignedInEventHandler> _logger;
        private readonly IMediator _mediator;

        public MemberSignedInEventHandler(ILogger<MemberSignedInEventHandler> logger,
                                          IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public Task Consume(ConsumeContext<DomainEventNotification<MemberSignedIn>> context)
        {
            var domainEvent = context.Message.DomainEvent;
            _logger.LogInformation($"Member Signed In: cognitoId = {domainEvent.CognitoId}, LastLogonUtc ={domainEvent.LastLogonUtc}");

            // use mediator to send UpdateMemberCommand to update member detail in the database
            UpdateMemberCommand updateCommand = new UpdateMemberCommand()
            {
                CognitoId = domainEvent.CognitoId, LastLogon = domainEvent.LastLogonUtc
            };

            _mediator.Publish<UpdateMemberCommand>(updateCommand);

            return Task.CompletedTask;
        }
    }
}
