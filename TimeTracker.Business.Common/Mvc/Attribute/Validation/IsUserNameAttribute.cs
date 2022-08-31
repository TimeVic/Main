using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsUserNameAttribute : RegularExpressionAttribute
    {
        public IsUserNameAttribute() : base(@"^[^\W_](?!.*?[._]{2})[\w.]{6,18}[^\W_]$")
        {  
        }
    }
}
