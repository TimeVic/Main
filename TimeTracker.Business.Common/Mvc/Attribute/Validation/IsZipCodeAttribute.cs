using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TimeTracker.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsZipCodeAttribute : ValidationAttribute
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
                if (IsZipCodeFormatValid(stringValue))
                {
                    return ValidationResult.Success;
                }    
            }
            return GetError(validationContext);
        }

        private bool IsZipCodeFormatValid(string zipCode)
        {
            string zipCodePattern = @"^\d{5}(-\d{4})?$";
            return Regex.IsMatch(zipCode, zipCodePattern);
        }

        private ValidationResult GetError(ValidationContext? validationContext) => new(
            $"Field \"{validationContext?.DisplayName}\" contains incorrect zip!"
        );
    }
}
