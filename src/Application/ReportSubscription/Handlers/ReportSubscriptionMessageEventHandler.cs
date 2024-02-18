using Accounts.Application.Common.Interfaces;
using Accounts.Application.Common.Models;
using Accounts.Application.ReportSubscription.Commands.SyncMemberData;
using Accounts.Application.Member.Handlers;
using Accounts.Domain.Events;
using MassTransit;
using MassTransit.Mediator;
using MassTransit.Middleware;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accounts.Domain.Entities.ReportSubscription;

namespace Accounts.Application.ReportSubscription.Handlers
{
    public class ReportSubscriptionMessageEventHandler : IConsumer<DomainEventNotification<ReportSubscriptionMessageEvent>>
    {
        private readonly ILogger<PasswordChangedEventHandler> _logger;
        private readonly IMediator _mediator;

        public ReportSubscriptionMessageEventHandler(ILogger<PasswordChangedEventHandler> logger,
                                      IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<DomainEventNotification<ReportSubscriptionMessageEvent>> context)
        {
            var @domainEvent = context.Message.DomainEvent;
            _logger.LogInformation($"Accounts - ReportSubscriptionMessageEvent = {@domainEvent.ToJson()}");
            var TableName = @domainEvent.Data.TableName;

            switch (TableName)
            {
                case "Member":
                    IEnumerable<ChangeDetail<ShopGoMember>> enumerableMember = GetUpdatedMemberDetails<ShopGoMember>(@domainEvent);
                    IEnumerable<ChangeDetail<ShopGoMember>> updatedMemberDetails = enumerableMember;
                    await Task.WhenAll(updatedMemberDetails.Select(x => PublishMemberDataSyncCommand(@domainEvent, x)));
                    break;

                case "Person":
                    IEnumerable<ChangeDetail<ShopGoPerson>> enumerablePerson = GetUpdatedMemberDetails<ShopGoPerson>(@domainEvent);
                    IEnumerable<ChangeDetail<ShopGoPerson>> updatedPerson = enumerablePerson;
                    await Task.WhenAll(updatedPerson.Select(x => PublishMemberDataSyncCommand(@domainEvent, x)));
                    break;

                case "CognitoMember":
                    IEnumerable<ChangeDetail<ShopGoCognitoMember>> enumerableCognitoMember = GetUpdatedMemberDetails<ShopGoCognitoMember>(@domainEvent);
                    IEnumerable<ChangeDetail<ShopGoCognitoMember>> updatedCognitoMember = enumerableCognitoMember;
                    await Task.WhenAll(updatedCognitoMember.Select(x => PublishMemberDataSyncCommand(@domainEvent, x)));
                    break;

                default:
                    _logger.LogWarning($"Table {TableName} is not handled");
                    break;
            }

        }

        private async Task PublishMemberDataSyncCommand<T>(ReportSubscriptionMessageEvent @event, ChangeDetail<T> memberChange)
        {
            SyncMemberDataCommand<T> syncCommand = new SyncMemberDataCommand<T>()
            {
                TableName = @event.Data.TableName,
                Changes = memberChange,
                LastModifiedDate = @event.Data.LastModifiedDate.ToUniversalTime()
            };

            await _mediator.Publish<SyncMemberDataCommand<T>>(syncCommand);
            _logger.LogInformation($"Accounts - publish SyncMemberDataCommand = {syncCommand.ToString()}");
        }

        public static IEnumerable<ChangeDetail<T>> GetUpdatedMemberDetails<T>(ReportSubscriptionMessageEvent @event)
            => JsonConvert.DeserializeObject<IEnumerable<ChangeDetail<T>>>(@event.Data.Changes);
    }
}
