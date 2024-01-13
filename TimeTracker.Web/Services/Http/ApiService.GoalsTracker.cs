using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<GoalsTrackerItemDto> GoalsTrackerAddGoalAsync(CreateItemRequest model)
        {
            var response = await PostAsync<GoalsTrackerItemDto>(ApiUrl.GoalsTrackerAddGoal, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
