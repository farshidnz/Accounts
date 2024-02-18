using Mapster;

namespace Accounts.Application.Common.Mappings
{
    public interface IMapFrom<T>
    {
        void Mapping(TypeAdapterConfig config) => config.ForType(typeof(T), GetType());
    }
}