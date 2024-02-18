using Mapster;

namespace Accounts.Application.Common.Mappings
{
    public interface IMapTo<T>
    {
        void Mapping(TypeAdapterConfig config) => config.ForType(GetType(), typeof(T));
    }
}