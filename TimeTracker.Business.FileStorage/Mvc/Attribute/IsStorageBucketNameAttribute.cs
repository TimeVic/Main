using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Business.FileStorage.Mvc.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsStorageBucketNameAttribute : RegularExpressionAttribute
    {
        public IsStorageBucketNameAttribute() : base(@"^[a-z]{1}[a-z-]{1,48}[a-z]{1}$")
        {  
        }
    }
}
