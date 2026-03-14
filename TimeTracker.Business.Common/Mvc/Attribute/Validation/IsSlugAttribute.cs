using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using TimeTracker.Business.Common.Resources;

namespace TimeTracker.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsSlugAttribute : ValidationAttribute
    {
        private static Regex _slugRegex = new(@"^[a-z0-9](?:[a-z0-9-]{0,78}[a-z0-9])?$");
        
        public IsSlugAttribute() : base()
        { }

        protected override ValidationResult? IsValid(object? value, ValidationContext? validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            var errorResult = new ValidationResult(string.Format(RG.Error_FieldMayContainOnlyTimeZoneId, validationContext?.DisplayName));
            if (value is string slug)
            {
                if (_slugRegex.IsMatch(slug))
                {
                    return ValidationResult.Success;
                }
            }
            return errorResult;
        }
    }
}
