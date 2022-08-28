using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsUrlAttribute : RegularExpressionAttribute
    {
        public IsUrlAttribute() : base(@"^http(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$")
        { }
    }
}