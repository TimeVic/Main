using System.ComponentModel.DataAnnotations;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Resources;

namespace TimeTracker.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsCorrectTimeEntryTimeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is TimeSpan dateValue)
            {
                if (dateValue < GlobalConstants.EndOfDay)
                {
                    return ValidationResult.Success;
                }
            }
            return GetError(validationContext);
        }

        private ValidationResult GetError(ValidationContext? validationContext) => new(
            string.Format(RG.Error_IncorrectTimeEntryTime, validationContext?.DisplayName)
        );
    }
}
