using System.ComponentModel.DataAnnotations;
using TimeTracker.Business.Common.Resources;

namespace TimeTracker.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsFutureOrNowDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return GetError(validationContext);
            }

            if (value is DateTime dateValue)
            {
                if (dateValue.Date >= DateTime.UtcNow.Date)
                {
                    return ValidationResult.Success;
                }
            }
            return GetError(validationContext);
        }

        private ValidationResult GetError(ValidationContext? validationContext) => new(
            string.Format(RG.Error_IncorrectDate, validationContext?.DisplayName)
        );
    }
}
