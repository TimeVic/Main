using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsTimeZoneAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is string stringValue)
            {
                if (string.IsNullOrEmpty(stringValue))
                {
                    return ValidationResult.Success;
                }
                if (IsValid(stringValue))
                {
                    return ValidationResult.Success;
                }    
            }
            return GetError(validationContext);
        }

        private bool IsValid(string timeZoneId)
        {
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return true;
            }
            catch (TimeZoneNotFoundException)
            {
                return false;
            }
            catch (InvalidTimeZoneException)
            {
                return false;
            }
        }

        private ValidationResult GetError(ValidationContext? validationContext) => new(
            $"Field \"{validationContext?.DisplayName}\" contains incorrect Time Zone Id!"
        );
    }
}
