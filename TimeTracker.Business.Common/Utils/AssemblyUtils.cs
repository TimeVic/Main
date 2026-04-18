using System.Reflection;

namespace TimeTracker.Business.Common.Utils
{
    public static class AssemblyUtils
    {
        public static string GetAssemblyPath(Assembly? assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();
            var path = Path.GetDirectoryName(assembly.Location);
            return path ?? string.Empty;
        }
    }
}
