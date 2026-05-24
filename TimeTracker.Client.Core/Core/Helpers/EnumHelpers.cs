using TimeTracker.Business.Extensions;

namespace TimeTracker.Client.Core.Core.Helpers;

public static class EnumHelpers
{
    public static string GetDisplayName(Type enumType, object value)
    {
        if (enumType is null)
            throw new ArgumentNullException(nameof(enumType));

        return enumType.GetDisplayName(value);
    } 
}
