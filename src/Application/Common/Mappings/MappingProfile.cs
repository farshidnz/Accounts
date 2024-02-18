using Mapster;
using System;
using System.Linq;
using System.Reflection;

namespace Accounts.Application.Common.Mappings
{
    public class MappingProfile : IRegister
    {
        private readonly Assembly assembly;

        public MappingProfile() : this(Assembly.GetExecutingAssembly())
        {
        }

        public MappingProfile(Assembly assembly)
        {
            this.assembly = assembly;
        }

        public void Register(TypeAdapterConfig config)
        {
            ApplyMappingsFromAssembly(config);
        }

        private void ApplyMappingsFromAssembly(TypeAdapterConfig config)
        {
            var types = assembly.GetExportedTypes()
                                .Where(t => t.GetInterfaces().Any(i =>
                                    i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IMapFrom<>) ||
                                    i.GetGenericTypeDefinition() == typeof(IMapTo<>))))
                                .ToList();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);

                MappingMethod(type)?.Invoke(instance, new object[] { config });
            }
        }

        private MethodInfo MappingMethod(Type type) => type.GetMethod("Mapping") ?? type.GetInterface("IMapFrom`1").GetMethod("Mapping");
    }
}