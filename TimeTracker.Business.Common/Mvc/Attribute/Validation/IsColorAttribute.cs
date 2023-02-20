using System.ComponentModel.DataAnnotations;
using System.Drawing;
using TimeTracker.Business.Common.Resources;

namespace TimeTracker.Business.Common.Mvc.Attribute.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IsColorAttribute : ValidationAttribute
    {
        public IsColorAttribute() : base()
        { }

        protected override ValidationResult? IsValid(object? value, ValidationContext? validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            var errorResult = new ValidationResult(string.Format(RG.Error_FieldMayContainOnlyColor, validationContext?.DisplayName));
            if (value is string colorString)
            {
                try
                {
                    var color = ColorTranslator.FromHtml(colorString);
                    if (color != Color.Empty)
                    {
                        return ValidationResult.Success;
                    }
                }
                catch (Exception e) {}
            }
            return errorResult;
        }
    }
}
