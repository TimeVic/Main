using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User
{
    public class RegistrationStep2Request : IRequest<RegistrationStep2ResponseDto>
    {
        [Required]
        [StringLength(32, MinimumLength = 6)]
        public string Password { get; set; }
        
        [Required]
        [StringLength(512, MinimumLength = 10)]
        public string Token { get; set; }
        
        [Required]
        [IsReCaptcha]
        public string ReCaptcha { get; set; }
    }
}
