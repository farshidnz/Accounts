using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using MassTransit.Mediator;
using Accounts.Application;
using Accounts.Application.Common.Interfaces;
using Accounts.Domain.Common;
using Accounts.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Accounts.Infrastructure.AWS
{
    public class LambdaEventTriggerHandler
    {
        private readonly IServiceCollection serviceCollection;

        // lambda can only invoke parameterless contructor
        public LambdaEventTriggerHandler()
        {
            serviceCollection = ConfigureServices();
        }

        // unit test constructor
        public LambdaEventTriggerHandler(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }

        private ServiceCollection ConfigureServices()
        {
            // add dependencies here
            var configuration = GetConfiguration();
            var services = new ServiceCollection();
            services.AddSingleton(configuration);
            services.AddLogging();
            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddSingleton<ICurrentUserService, LambdaCurrentUserService>();
            services.AddApplication();
            services.AddInfrastructure(configuration);
            return services;
        }

        public IConfiguration GetConfiguration()
        {
            var assembly = Assembly.Load("Accounts.API");
            var appSettingDir = Path.GetDirectoryName(assembly.Location);
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"{appSettingDir}/appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"{appSettingDir}/appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ReadEvents(Amazon.Lambda.SQSEvents.SQSEvent evnt, ILambdaContext context)
        {
            await using var serviceProvider = BuildServiceProvider();
            await Task.WhenAll(evnt.Records.Select(x => ProcessEvent(x, context, serviceProvider)));
        }

        public virtual ServiceProvider BuildServiceProvider() => serviceCollection.BuildServiceProvider();

        private async Task ProcessEvent(Amazon.Lambda.SQSEvents.SQSEvent.SQSMessage message, ILambdaContext context, ServiceProvider serviceProvider)
        {
            var eventTypeAttribute = EventMessageAttributes.EventType.ToString();
            if (!message.MessageAttributes.TryGetValue(eventTypeAttribute, out Amazon.Lambda.SQSEvents.SQSEvent.MessageAttribute eventTypeName))
            {
                throw new InvalidOperationException($"Invalid event message, missing required Message Attribute \"{eventTypeAttribute}\"");
            }

            var lambdaTriggeredEventTypes = GetLambdaTriggeredEventTypes(serviceProvider);
            if (!lambdaTriggeredEventTypes.TryGetValue(eventTypeName.StringValue, out Type eventType))
            {
                throw new NotSupportedException($"Unsupported event type: {eventTypeName.StringValue}");
            }

            var domainEvent = ConvertToDomainEvent(message, eventType, serviceProvider);

            await PublishToInternalEventHandlers(domainEvent, message, serviceProvider);

            await Task.CompletedTask;
        }

        private Dictionary<string, Type> GetLambdaTriggeredEventTypes(ServiceProvider serviceProvider)
        {
            var eventServiceFactory = GetEventServiceFactory(serviceProvider);
            return eventServiceFactory.GetAWSEventReaders(AwsEventReadMode.LambdaTrigger)
                                      .SelectMany(x => x.EventTypes)
                                      .ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase);
        }

        public virtual IAWSEventServiceFactory GetEventServiceFactory(ServiceProvider serviceProvider) => serviceProvider.GetRequiredService<IAWSEventServiceFactory>();

        public virtual IMediator GetMediator(ServiceProvider serviceProvider) => serviceProvider.GetRequiredService<IMediator>();

        public virtual ILogger<LambdaEventTriggerHandler> GetLogger(ServiceProvider serviceProvider) => serviceProvider.GetRequiredService<ILogger<LambdaEventTriggerHandler>>();

        private async Task PublishToInternalEventHandlers(DomainEvent domainEvent, Amazon.Lambda.SQSEvents.SQSEvent.SQSMessage message, ServiceProvider serviceProvider)
        {
            try
            {
                await GetMediator(serviceProvider).PublishEvent(domainEvent, message);
            }
            catch (Exception e)
            {
                var errorMessage = $"Error handling domain event: {domainEvent.ToJson()}";
                GetLogger(serviceProvider).LogError(e, errorMessage);
                throw new Exception($"{errorMessage}, error: {e}");
            }
        }

        private DomainEvent ConvertToDomainEvent(Amazon.Lambda.SQSEvents.SQSEvent.SQSMessage message, Type eventType, ServiceProvider serviceProvider)
        {
            try
            {
                return JsonConvert.DeserializeObject(message.Body, eventType) as DomainEvent;
            }
            catch (Exception e)
            {
                var errorMessage = $"Failed to deserialise message : {message.Body}, into event type {eventType}.";
                GetLogger(serviceProvider).LogError(e, errorMessage);
                throw new Exception($"{errorMessage}, error: {e}");
            }
        }
    }
}