using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User
{
    public class ResetPasswordStep1Request : IRequest
    {   
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [IsReCaptcha]
        public string ReCaptcha { get; set; } = string.Empty;
    }
}
