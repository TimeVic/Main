﻿using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    public class RegistrationStep2RequestHandler : IAsyncRequestHandler<RegistrationStep2Request, RegistrationStep2ResponseDto>
    {
        private readonly IRegistrationService _registrationService;
        private readonly IJwtAuthService _jwtAuthService;
        private readonly IMapper _mapper;

        public RegistrationStep2RequestHandler(
            IRegistrationService registrationService,
            IJwtAuthService jwtAuthService,
            IMapper mapper
        )
        {
            _registrationService = registrationService;
            _jwtAuthService = jwtAuthService;
            _mapper = mapper;
        }
    
        public async Task<RegistrationStep2ResponseDto> ExecuteAsync(RegistrationStep2Request request)
        {
            var user = await _registrationService.ActivateUser(request.Token, request.Password);
            return new RegistrationStep2ResponseDto()
            {
                JwtToken = _jwtAuthService.BuildJwt(user.Id),
                User = _mapper.Map<UserDto>(user)
            };
        }
    }
}
