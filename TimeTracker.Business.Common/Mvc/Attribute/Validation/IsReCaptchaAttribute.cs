using System.ComponentModel.DataAnnotations;
using TimeTracker.Business.Common.Resources;
using TimeTracker.Business.Common.Services.Web.ReCaptcha;
using Microsoft.Extensions.DependencyInjection;

namespace TimeTracker.Business.Common.Mvc.Attribute.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class IsReCaptchaAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var reCaptchaService = validationContext.GetService<IReCaptchaService>();
        if (reCaptchaService == null)
        {
            // This is frontend application
            return ValidationResult.Success;
        }

        if (value == null)
        {
            return GetError(validationContext);
        }

        if (value is string stringValue)
        {
            var isValid = reCaptchaService.ValidateAsync(stringValue).Result;
            return isValid ? ValidationResult.Success : GetError(validationContext);
        }
        return GetError(validationContext);
    }

    private ValidationResult GetError(ValidationContext validationContext) => new(
        string.Format(RG.Error_IncorrectReCaptchaToken, validationContext.DisplayName)
    );
}
