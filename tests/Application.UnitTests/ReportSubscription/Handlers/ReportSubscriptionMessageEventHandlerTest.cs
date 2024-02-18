using Accounts.Application.Common.Models;
using Accounts.Application.Member.Commands.UpdateMember.v1;
using Accounts.Application.ReportSubscription.Commands.SyncMemberData;
using Accounts.Domain.Entities.ReportSubscription;
using Accounts.Domain.Enums;
using Accounts.Domain.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Application.UnitTests.ReportSubscription.Handlers
{
    [TestFixture]
    internal class ReportSubscriptionMessageEventHandlerTest : TestBase
    {
        [Test]
        public async Task ReportSubscriptionMessageEventHandler_ShouldHandleShopGoMemberTableAndSendSyncMemberDataCommand()
        {
            var state = new TestState();

            var context = new Mock<ConsumeContext<DomainEventNotification<ReportSubscriptionMessageEvent>>>();
            context.Setup(c => c.Message).Returns(new DomainEventNotification<ReportSubscriptionMessageEvent>(new ReportSubscriptionMessageEvent
            {
                Data =  new ReportSubscriptionMessageEventData
                {
                    UniqueId = Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746"),
                    TableName = "Member",
                    Changes = "[{\"Id\":1001481018,\"Action\":\"Update\",\"Before\":{\"MemberId\":1001481018,\"ClientId\":1000034,\"Status\":1,\"FirstName\":\"Feo\",\"LastName\":\"test\",\"PostCode\":\"AZ/VSebj5bKb38tzawlAZ6qjMp91kC3O2kuVOlToBMGreNBPZXAqDOmzxIHFglajTgD0xnwhp+09h5qqChydoKQ=\",\"Mobile\":\"AQYAR6vJ68cFC8LEhzCsAhpKCNeF7dpBlcnElopO0CsKL0uhUiZtuKB1KCMQX9Lg8Mhy0PPbbfjzz8bTMwPXkbwpCSf+O/oUlhxMEhDu5YYy\",\"Email\":\"felix.pan+2030@cashrewards.com\",\"UserPassword\":\"47226a7xCpzoztk6Rrrb6wfFXp2kAAr5k4URpxpZ6e0=\",\"SaltKey\":\"APDFZXPOPUXJIJCHRWMH\",\"ReceiveNewsLetter\":false,\"ClickWindowActive\":false,\"PopUpActive\":false,\"IsValidated\":true,\"IsResetPassword\":false,\"RequiredLogin\":false,\"IsAvailable\":false,\"ActivateBy\":\"7/14/2032 7:20:53 PM\",\"DateJoined\":\"7/14/2022 7:20:53 PM\",\"HashedEmail\":\"Sw0txB8kACU7hlPkUzNVrkjkartWJqymS1L3A9S4Svo73xlTE=\",\"HashedMobile\":\"izug9uS0cduq1FKUx0QtcMUaJARU6QuCcQZIpJOZTOY=\",\"CommunicationsEmail\":\"felix.pan+2030@cashrewards.com\",\"MemberNewId\":{\"Guid\":\"0f7b2d70-8d94-4514-a574-bc6619ea0d33\"},\"AutoCreated\":false},\"After\":{\"MemberId\":1001481018,\"ClientId\":1000034,\"Status\":1,\"FirstName\":\"Feo\",\"LastName\":\"ggg\",\"PostCode\":\"AZ/VSebj5bKb38tzawlAZ6qjMp91kC3O2kuVOlToBMGreNBPZXAqDOmzxIHFglajTgD0xnwhp+09h5qqChydoKQ=\",\"Mobile\":\"AQYAR6vJ68cFC8LEhzCsAhpKCNeF7dpBlcnElopO0CsKL0uhUiZtuKB1KCMQX9Lg8Mhy0PPbbfjzz8bTMwPXkbwpCSf+O/oUlhxMEhDu5YYy\",\"Email\":\"felix.pan+2030@cashrewards.com\",\"UserPassword\":\"47226a7xCpzoztk6Rrrb6wfFXp2kAAr5k4URpxpZ6e0=\",\"SaltKey\":\"APDFZXPOPUXJIJCHRWMH\",\"ReceiveNewsLetter\":false,\"ClickWindowActive\":false,\"PopUpActive\":false,\"IsValidated\":true,\"IsResetPassword\":false,\"RequiredLogin\":false,\"IsAvailable\":false,\"ActivateBy\":\"7/14/2032 7:20:53 PM\",\"DateJoined\":\"7/14/2022 7:20:53 PM\",\"HashedEmail\":\"Sw0txB8kACU7hlPkUzNVrkjkartWJqymS1L3A9S4Svo73xlTE=\",\"HashedMobile\":\"izug9uS0cduq1FKUx0QtcMUaJARU6QuCcQZIpJOZTOY=\",\"CommunicationsEmail\":\"felix.pan+2030@cashrewards.com\",\"MemberNewId\":{\"Guid\":\"0f7b2d70-8d94-4514-a574-bc6619ea0d33\"},\"AutoCreated\":false},\"ModifiedFields\":[{\"FieldName\":\"LastName\"}]}]",
                    LastModifiedDate = DateTime.Parse("2022-12-28 08:00:00")
                }
            }, null));
            state._mediator.Setup(r => r.Publish<SyncMemberDataCommand<ShopGoMember>>(It.IsAny<SyncMemberDataCommand<ShopGoMember>>(), default));

            await state._ReportSubscriptionMessageEventHandler.Consume(context.Object);
            state._mediator.Verify(c => c.Publish<SyncMemberDataCommand<ShopGoMember>>(It.IsAny<SyncMemberDataCommand<ShopGoMember>>(), default), Times.Once);
        }

        [Test]
        public async Task ReportSubscriptionMessageEventHandler_ShouldHandleShopGoPersonTableAndSendSyncMemberDataCommand()
        {
            var state = new TestState();

            var context = new Mock<ConsumeContext<DomainEventNotification<ReportSubscriptionMessageEvent>>>();
            context.Setup(c => c.Message).Returns(new DomainEventNotification<ReportSubscriptionMessageEvent>(new ReportSubscriptionMessageEvent
            {
                Data = new ReportSubscriptionMessageEventData
                {
                    UniqueId = Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746"),
                    TableName = "Person",
                    Changes = "[{\"Id\":764256,\"Action\":\"Update\",\"Before\":{\"PersonId\":764256,\"CognitoId\":{\"Guid\":\"0f7b2d70-8d94-4514-a574-bc6619ea0d33\"},\"PremiumStatus\":1,\"TrustScore\":0,\"OriginationSource\":\"\",\"CreatedDateUTC\":\"2022-07-14 19:20:54.0300000\",\"UpdatedDateUTC\":\"2022-07-14 09:20:54.0570000\"},\"After\":{\"PersonId\":764256,\"CognitoId\":{\"Guid\":\"0f7b2d70-8d94-4514-a574-bc6619ea0d33\"},\"PremiumStatus\":0,\"TrustScore\":0,\"OriginationSource\":\"\",\"CreatedDateUTC\":\"2022-07-14 19:20:54.0300000\",\"UpdatedDateUTC\":\"2022-07-14 09:20:54.0570000\"},\"ModifiedFields\":[{\"FieldName\":\"PremiumStatus\"}]}]",
                    LastModifiedDate = DateTime.Parse("2022-12-28 08:00:00")
                }
            }, null));
            state._mediator.Setup(r => r.Publish<SyncMemberDataCommand<ShopGoPerson>>(It.IsAny<SyncMemberDataCommand<ShopGoPerson>>(), default));

            await state._ReportSubscriptionMessageEventHandler.Consume(context.Object);
            state._mediator.Verify(c => c.Publish<SyncMemberDataCommand<ShopGoPerson>>(It.IsAny<SyncMemberDataCommand<ShopGoPerson>>(), default), Times.Once);
        }

        [Test]
        public async Task ReportSubscriptionMessageEventHandler_ShouldHandleShopGoCognitoMemberAndSendSyncMemberDataCommand()
        {
            var state = new TestState();

            var context = new Mock<ConsumeContext<DomainEventNotification<ReportSubscriptionMessageEvent>>>();
            context.Setup(c => c.Message).Returns(new DomainEventNotification<ReportSubscriptionMessageEvent>(new ReportSubscriptionMessageEvent
            {
                Data = new ReportSubscriptionMessageEventData
                {
                    UniqueId = Guid.Parse("a34d8785-f34b-42fc-ab0b-36f0b73dc746"),
                    TableName = "CognitoMember",
                    Changes = "[{\"Id\":1000734262,\"Action\":\"Update\",\"Before\":{\"MappingId\":1000734262,\"CognitoId\":\"0f7b2d70-8d94-4514-a574-bc6619ea0d33\",\"MemberId\":1001481018,\"CognitopoolId\":\"ap-southeast-2_9q6TXai99\",\"MemberNewId\":{\"Guid\":\"05BEC200-F592-4FF7-9A70-AA965754F783\"},\"CreatedAt\":\"2022-07-14 19:20:54.0300000\",\"UpdatedAt\":\"2022-07-14 09:20:54.0570000\",\"Status\":1,PersonId:\"764256\"},\"After\":{\"MappingId\":1000734262,\"CognitoId\":\"0f7b2d70-8d94-4514-a574-bc6619ea0d33\",\"MemberId\":1001481018,\"CognitopoolId\":\"ap-southeast-2_9q6TXai99\",\"MemberNewId\":{\"Guid\":\"05BEC200-F592-4FF7-9A70-AA965754F783\"},\"CreatedAt\":\"2022-07-14 19:20:54.0300000\",\"UpdatedAt\":\"2022-07-14 08:20:54.0570000\",\"Status\":0,PersonId:\"764256\"},\"ModifiedFields\":[{\"FieldName\":\"Status\"},{\"FieldName\":\"UpdatedAt\"}]}]",
                    LastModifiedDate = DateTime.Parse("2022-12-28 08:00:00")
                }
            }, null));
            state._mediator.Setup(r => r.Publish<SyncMemberDataCommand<ShopGoCognitoMember>>(It.IsAny<SyncMemberDataCommand<ShopGoCognitoMember>>(), default));

            await state._ReportSubscriptionMessageEventHandler.Consume(context.Object);
            state._mediator.Verify(c => c.Publish<SyncMemberDataCommand<ShopGoCognitoMember>>(It.IsAny<SyncMemberDataCommand<ShopGoCognitoMember>>(), default), Times.Once);
        }
    }
}
