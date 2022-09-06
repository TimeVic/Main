using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace TimeTracker.Web.Core.Extensions
{
    public static class NavigationManagerExtensions
    {
        public static string GetPath(this NavigationManager manager)
        {
            return new Uri(manager.Uri).AbsolutePath;
        }
        
        public static string GetPath(this LocationChangedEventArgs manager)
        {
            return new Uri(manager.Location).AbsolutePath;
        }
        
        public static Dictionary<string, StringValues> GetQueryParameters(this NavigationManager manager)
        {
            var uri = manager.ToAbsoluteUri(manager.Uri);
            return QueryHelpers.ParseQuery(uri.Query);
        }
        
        public static List<string> GetQueryParameterValue(this NavigationManager manager, string name)
        {
            var parameters = manager.GetQueryParameters();
            if (parameters.TryGetValue(name, out var values))
            {
                return values.ToList();
            }
            return new List<string>();
        }
        
        public static bool TryGetQueryValue<T>(this NavigationManager navManager, string key, out T value)
        {
            var values = navManager.GetQueryParameterValue(key);

            var valueFromQueryString = values.FirstOrDefault();
            if (!string.IsNullOrEmpty(valueFromQueryString))
            {
                if (typeof(T) == typeof(int) && int.TryParse(valueFromQueryString, out var valueAsInt))
                {
                    value = (T)(object)valueAsInt;
                    return true;
                }
                
                if (typeof(T) == typeof(long) && long.TryParse(valueFromQueryString, out var valueAsLong))
                {
                    value = (T)(object)valueAsLong;
                    return true;
                }
                
                if (typeof(T) == typeof(short) && short.TryParse(valueFromQueryString, out var valueAsShort))
                {
                    value = (T)(object)valueAsShort;
                    return true;
                }

                if (typeof(T) == typeof(string))
                {
                    value = (T)(object)valueFromQueryString.ToString();
                    return true;
                }
                
                if (typeof(T) == typeof(bool) && bool.TryParse(valueFromQueryString, out var boolValue))
                {
                    value = (T)(object)boolValue;
                    return true;
                }

                if (typeof(T) == typeof(decimal) && decimal.TryParse(valueFromQueryString, out var valueAsDecimal))
                {
                    value = (T)(object)valueAsDecimal;
                    return true;
                }
                
                if (typeof(T) == typeof(LogLevel))
                {
                    Enum.TryParse<LogLevel>(valueFromQueryString, out var enumValue);
                    value = (T)(object)enumValue;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
