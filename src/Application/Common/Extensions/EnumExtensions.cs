using System;

namespace Accounts.Application.Common.Extensions
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string value) where T : struct
        {
            return Enum.TryParse<T>(value, true, out T result) ? result : default;
        }
    }
}