using System.Reflection;

namespace TimeTracker.Business.Common.Utils
{
    public static class IoUtils
    {
        public static string GetResourcePath(Assembly assembly, string resourcePath)
        {
            
            List<string> resourceNames = new List<string>(assembly.GetManifestResourceNames());
            resourcePath = resourcePath.Replace(@"/", ".");
            var resolvedResourcePath = resourceNames.FirstOrDefault(r => r.Contains(resourcePath));

            if (resolvedResourcePath == null)
                throw new FileNotFoundException("Resource not found");

            return resolvedResourcePath;
        }

        public static string GetResourcePath(string resourcePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return GetResourcePath(assembly, resourcePath);
        }

        public static Stream GetResourceStream(string filePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var resourcePath = GetResourcePath(filePath);
            return assembly.GetManifestResourceStream(resourcePath)
                   ?? throw new FileNotFoundException("Resource not found");
        }

        public static string GetResourceAsString(Assembly assembly, string filePath)
        {
            var resourcePath = GetResourcePath(filePath);
            using (var stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException("Resource not found");
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static string GetResourceAsString(string filePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return GetResourceAsString(assembly, filePath);
        }
    }
}
