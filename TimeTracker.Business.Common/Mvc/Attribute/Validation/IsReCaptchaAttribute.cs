using System.ComponentModel.DataAnnotations;
using TimeTracker.Business.Common.Resources;
using TimeTracker.Business.Common.Services.Web.ReCaptcha;
using Microsoft.Extensions.DependencyInjection;

namespace TimeTracker.Business.Common.Mvc.Attribute.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class IsReCaptchaAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return ValidationResult.Success;
    }

    private ValidationResult GetError(ValidationContext validationContext) => new(
        string.Format(RG.Error_IncorrectReCaptchaToken, validationContext.DisplayName)
    );
}
