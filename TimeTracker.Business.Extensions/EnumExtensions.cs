using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Business.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum enumValue)  
    {  
        var genericEnumType = enumValue.GetType();
        var memberInfo = genericEnumType.GetMember(enumValue.ToString());
        if (memberInfo.Any())
        {
            var attribs = memberInfo[0].GetCustomAttributes(
                typeof(DescriptionAttribute),
                false
            );
            if (attribs.Any())
            {
                return ((DescriptionAttribute)attribs.ElementAt(0)).Description;
            }
            attribs = memberInfo[0].GetCustomAttributes(
                typeof(DisplayAttribute),
                false
            );
            if (attribs.Any())
            {
                var firstElement = attribs.ElementAtOrDefault(0);
                if (firstElement != null)
                {
                    var name = ((DisplayAttribute) firstElement).GetName();
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new ArgumentNullException();
                    }
                    return name;
                }
                throw new ArgumentNullException();
            }
        }
        return enumValue.ToString();
    }
    
    public static string GetDisplayName(this Enum enumValue)  
    {  
        var genericEnumType = enumValue.GetType();
        var memberInfo = genericEnumType.GetMember(enumValue.ToString());
        if (memberInfo.Any())
        {
            var attribs = memberInfo[0].GetCustomAttributes(
                typeof(DescriptionAttribute),
                false
            );
            if (attribs.Any())
            {
                return ((DescriptionAttribute)attribs.ElementAt(0)).Description;
            }
            attribs = memberInfo[0].GetCustomAttributes(
                typeof(DisplayAttribute),
                false
            );
            if (attribs.Any())
            {
                return ((DisplayAttribute)attribs.ElementAt(0)).GetName() ?? enumValue.ToString();
            }
        }
        return enumValue.ToString();
    }
}
