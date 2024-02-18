using Newtonsoft.Json;
using System.IO;

namespace Accounts.Application.UnitTests.Member.Helpers
{
    public static class TestDataLoader
    {
        public static T Load<T>(string testDataFileName, JsonSerializerSettings settings = null) => JsonConvert.DeserializeObject<T>(File.ReadAllText(testDataFileName.Replace("\\", "/")), settings);

        public static string Load(string testDataFileName) => File.ReadAllText(testDataFileName.Replace("\\", "/"));
    }
}
