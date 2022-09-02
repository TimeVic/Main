using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User
{
    public class LoginRequest : IRequest<LoginResponseDto>
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [StringLength(256, MinimumLength = 6)]
        public string Password { get; set; }
        
        [Required]
        [IsReCaptcha]
        public string ReCaptcha { get; set; }
    }
}
