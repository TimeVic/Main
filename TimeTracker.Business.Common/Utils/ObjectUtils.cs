using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Business.Common.Utils;

public class ObjectUtils
{
    public static string GetDisplayName(object enumValue)  
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
                return ((DisplayAttribute)attribs.ElementAt(0)).GetName();
            }
        }
        return enumValue.ToString();
    }
}
