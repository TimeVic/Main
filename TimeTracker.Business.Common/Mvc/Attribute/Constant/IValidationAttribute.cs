namespace TimeTracker.Business.Common.Mvc.Attribute.Constant
{
    public interface IValidationAttribute
    {
        bool IsValid(string Value);
    }
}
