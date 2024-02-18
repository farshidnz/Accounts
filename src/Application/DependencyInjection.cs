using Accounts.Application.Common.Behaviours;
using FluentValidation;
using Mapster;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Accounts.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            RegisterMappingProfiles();
            
            services.AddMediator(cfg =>
            {
                cfg.AddConsumers(Assembly.GetExecutingAssembly());

                cfg.ConfigureMediator((context, mcfg) =>
                {
                    RegisterMassTransitFilters(context, mcfg);
                });
            });

            return services;
        }

        private static void RegisterMappingProfiles()
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        }

        private static void RegisterMassTransitFilters(IMediatorRegistrationContext context, IMediatorConfigurator mcfg)
        {
            // order of execution of the filters is important
            mcfg.UseConsumeFilter(typeof(UnhandledExceptionBehaviour<>), context);

            // is logging of commands, queries and events prior to handling them required ?
            // mcfg.UseConsumeFilter(typeof(LoggingBehaviour<>), context);

            // is custome authorisation logic for commands, queries and events required ?
            // mcfg.UseConsumeFilter(typeof(AuthorisationBehaviour<>), context);

            // enable validators for commands, queries and events when present
            mcfg.UseConsumeFilter(typeof(ValidationBehaviour<>), context);

            // monitor poorly performing commands, queries and events
            mcfg.UseConsumeFilter(typeof(PerformanceBehaviour<>), context);
        }
    }
}