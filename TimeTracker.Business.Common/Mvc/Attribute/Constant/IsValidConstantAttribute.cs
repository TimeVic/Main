using System.ComponentModel.DataAnnotations;
using TimeTracker.Business.Common.Resources;

namespace TimeTracker.Business.Common.Mvc.Attribute.Constant
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsValidConstantAttribute : ValidationAttribute
    {
        private IValidationAttribute ValidationInstance;

        public IsValidConstantAttribute(Type ClassType)
        {
            ValidationInstance = Activator.CreateInstance(ClassType) as IValidationAttribute;
            if (ValidationInstance == null)
            {
                throw new Exception($"{ValidationInstance} should implement IValidationAttribute interface");
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var stringValue = value as string;
            if (stringValue != null || value is int || value is long)
            {
                if (!ValidationInstance.IsValid(stringValue))
                {
                    return new ValidationResult(
                        string.Format(RG.Error_IncorrectConstantValue, validationContext.DisplayName)
                    );
                }
            }
            return ValidationResult.Success;
        }
    }
}
