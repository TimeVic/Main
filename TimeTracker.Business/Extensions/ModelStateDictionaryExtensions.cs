using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TimeTracker.Business.Extensions
{
    public static class ModelStateDictionaryExtensions
    {
        public static List<string> GetErrorsFromModelState(this ModelStateDictionary modelState)
        {
            var errors = new List<string>();
            foreach (var value in modelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    var errorMessage = !string.IsNullOrEmpty(error.ErrorMessage)
                        ? error.ErrorMessage
                        : error.Exception.Message;
                    errors.Add(errorMessage);
                }
            }
            return errors;
        }
    }
}