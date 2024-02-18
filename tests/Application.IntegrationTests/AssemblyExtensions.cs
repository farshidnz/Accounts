using System.IO;
using System.Reflection;

namespace Accounts.Application.IntegrationTests
{
    public static class AssemblyExtensions
    {
        public static string Folder(this Assembly assembly) => Path.GetDirectoryName(assembly.Location);
    }
}
