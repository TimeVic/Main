using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User
{
    public class LoginRequest : IRequest<LoginResponseDto>
    {
        [Required]
        [StringLength(5048, MinimumLength = 50)]
        public string Pem { get; set; }
        
        [Required]
        [StringLength(256, MinimumLength = 3)]
        public string Password { get; set; }
        
        [Required]
        [IsReCaptcha]
        public string ReCaptcha { get; set; }
    }
}
