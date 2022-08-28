using System.ComponentModel.DataAnnotations;
using TimeTracker.Business.Common.Resources;

namespace TimeTracker.Business.Common.Mvc.Attribute.Constant
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsValidConstantArrayAttribute : ValidationAttribute
    {
        private IValidationAttribute ValidationInstance;

        public IsValidConstantArrayAttribute(Type ClassType)
        {
            ValidationInstance = Activator.CreateInstance(ClassType) as IValidationAttribute;
            if (ValidationInstance == null)
            {
                throw new Exception($"{ValidationInstance} should implement IValidationAttribute interface");
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string[] arrayToValidate)
            {
                if (arrayToValidate.Any(item => !ValidationInstance.IsValid(item)))
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
