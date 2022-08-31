using Api.Requests.Abstractions;

namespace TimeTracker.Api.Controllers.Public.User.Actions
{
    // public class RegistrationStep1RequestHandler : IAsyncRequestHandler<RegistrationStep1Request>
    // {
    //     private readonly IAsyncQueryBuilder _queryBuilder;
    //     private readonly IUserService _userService;
    //     private readonly IKafkaProducerService _kafkaProducerService;
    //     private readonly string _frontendUrl;
    //
    //     public RegistrationStep1RequestHandler(
    //         IAsyncQueryBuilder queryBuilder, 
    //         IUserService userService,
    //         IKafkaProducerService kafkaProducerService,
    //         IConfiguration configuration
    //     )
    //     {
    //         _queryBuilder = queryBuilder;
    //         _userService = userService;
    //         _kafkaProducerService = kafkaProducerService;
    //         _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
    //     }
    //
    //     public async Task ExecuteAsync(RegistrationStep1Request request)
    //     {
    //         var currentUser = await _queryBuilder.For<UserEntity>()
    //             .WithAsync(new UserGetByCriteria(null, request.Email));
    //         if (currentUser?.Status == UserStatus.Active)
    //         {
    //             throw new RecordIsExistsException("User is exists");
    //         }
    //
    //         if (currentUser == null)
    //         {
    //             currentUser = await _userService.CreatePendingUser(request.Email);    
    //         }
    //         await _kafkaProducerService.ProduceNotificationMessageAsync(
    //             new RegistrationNotificationContext(
    //                 currentUser.Email, 
    //                 _frontendUrl,
    //                 currentUser.VerificationToken
    //             )
    //         );
    //     }
    // }
}
