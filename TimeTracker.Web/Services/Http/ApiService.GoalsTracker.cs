using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<GoalsTrackerDto> GoalsTrackerLoadAsync(long workspaceId, DateTime date)
        {
            var response = await PostAsync<GoalsTrackerDto>(ApiUrl.GoalsTrackerGet, new GetRequest()
            {
                Date = date,
                WorkspaceId = workspaceId
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<GoalsTrackerItemDto> GoalsTrackerCreateItemAsync(long trackerId, string name, int numberOfTimes)
        {
            var response = await PostAsync<GoalsTrackerItemDto>(ApiUrl.GoalsTrackerItemCreate, new CreateItemRequest()
            {
                GoalsTrackerId = trackerId,
                NumberOfTimes = numberOfTimes,
                Name = name
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<GoalsTrackerItemDto> GoalsTrackerUpdateItemAsync(long itemId, string name, int numberOfTimes)
        {
            var response = await PostAsync<GoalsTrackerItemDto>(ApiUrl.GoalsTrackerItemUpdate, new UpdateItemRequest()
            {
                GoalsTrackerItemId = itemId,
                NumberOfTimes = numberOfTimes,
                Name = name
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task GoalsTrackerDeleteItemAsync(long itemId)
        {
            var response = await PostAsync<object>(ApiUrl.GoalsTrackerItemDelete, new DeleteItemRequest()
            {
                Id = itemId
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }
        }
        
        public async Task<GoalsTrackerCompletionMarkerDto> GoalsTrackerSetCompletionAsync(long itemId, int dayOfMonth, bool isChecked)
        {
            var response = await PostAsync<GoalsTrackerCompletionMarkerDto>(ApiUrl.GoalsTrackerItemSetCompletion, new SetCompletionRequest()
            {
                GoalsTrackerItemId = itemId,
                DayOfMonth = dayOfMonth,
                IsChecked = isChecked
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
