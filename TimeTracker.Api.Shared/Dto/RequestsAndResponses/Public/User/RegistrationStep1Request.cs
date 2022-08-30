using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User
{
    public class RegistrationStep1Request : IRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [IsReCaptcha]
        public string ReCaptcha { get; set; }
    }
}
