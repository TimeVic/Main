using System.ComponentModel.DataAnnotations;
using TimeTracker.Business.Common.Resources;

namespace TimeTracker.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsTimeZoneIdAttribute : ValidationAttribute
    {
        public IsTimeZoneIdAttribute() : base()
        { }

        protected override ValidationResult? IsValid(object? value, ValidationContext? validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            var errorResult = new ValidationResult(string.Format(RG.Error_FieldMayContainOnlyTimeZoneId, validationContext?.DisplayName));
            if (value is string timeZoneId)
            {
                try
                {
                    TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                    return ValidationResult.Success;
                }
                catch (InvalidTimeZoneException e) {}
                catch (TimeZoneNotFoundException e) {}
            }
            return errorResult;
        }
    }
}
