using Api.Requests.Abstractions;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    // public class RegistrationStep2RequestHandler : IAsyncRequestHandler<RegistrationStep2Request, RegistrationStep2ResponseDto>
    // {
    //     private readonly IAsyncQueryBuilder _queryBuilder;
    //     private readonly IJwtAuthService _jwtAuthService;
    //     private readonly IUserService _userService;
    //
    //     public RegistrationStep2RequestHandler(
    //         IAsyncQueryBuilder queryBuilder, 
    //         IJwtAuthService jwtAuthService,
    //         IUserService userService
    //     )
    //     {
    //         _queryBuilder = queryBuilder;
    //         _jwtAuthService = jwtAuthService;
    //         _userService = userService;
    //     }
    //
    //     public async Task<RegistrationStep2ResponseDto> ExecuteAsync(RegistrationStep2Request request)
    //     {
    //         var user = await _queryBuilder.For<UserEntity>()
    //             .WithAsync(new UserGetPendingCriteria(request.Token));
    //         if (user == null)
    //         {
    //             throw new EntityIsNotExistException();
    //         }
    //
    //         var (_, pemFile) = await _userService.ActivateUser(user.Id, request.Password);
    //
    //         var asymmetricEncryptor = AsymmetricEncryptor.ReadFromPem(pemFile, request.Password);
    //         return new RegistrationStep2ResponseDto()
    //         {
    //             JwtToken = _jwtAuthService.BuildJwt(user.Id),
    //             Pem = pemFile,
    //             PrivateKeyBase64 = Convert.ToBase64String(asymmetricEncryptor.GetPrivateKeyBytes()),
    //         };
    //     }
    // }
}
