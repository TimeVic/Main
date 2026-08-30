using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TimeTracker.Business.Common.Mvc.Attribute.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class IsLoginAttribute : ValidationAttribute
{
    private static readonly Regex _loginRegex = new(@"^[a-z0-9_]{3,60}$", RegexOptions.Compiled);
    
    public IsLoginAttribute() : base()
    { }

    protected override ValidationResult? IsValid(object? value, ValidationContext? validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }
        var memberNames = validationContext?.MemberName != null ? new[] { validationContext.MemberName } : null;
        var errorResult = new ValidationResult(
            $"Field \"{validationContext?.DisplayName ?? "Login"}\" may contain only lowercase letters, digits and underscores, and be between 3 and 60 characters long.",
            memberNames
        );
        if (value is string login)
        {
            login = login.TrimStart('@');
            if (_loginRegex.IsMatch(login))
            {
                return ValidationResult.Success;
            }
        }
        return errorResult;
    }
}
