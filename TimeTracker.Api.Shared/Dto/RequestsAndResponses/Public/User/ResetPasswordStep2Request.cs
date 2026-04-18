using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User
{
    public class ResetPasswordStep2Request : IRequest
    {   
        [Required]
        [StringLength(1024, MinimumLength = 100)]
        public string VerficationToken { get; set; } = string.Empty;
        
        [Required]
        [StringLength(256, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [IsReCaptcha]
        public string ReCaptcha { get; set; } = string.Empty;
    }
}
