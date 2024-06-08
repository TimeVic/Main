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
        
        public async Task GoalsTrackerChangePositionsAsync(
            long workspaceId,
            DateTime date,
            ICollection<GoalsTrackerItemDto> items
        )
        {
            await PostAsync<object>(ApiUrl.GoalsTrackerChangePositions, new ChangePositionsRequest()
            {
                Date = date,
                WorkspaceId = workspaceId,
                Positions = items.ToDictionary(x => x.Id, y => y.Position)
            });
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
        
        public async Task<GoalsTrackerItemDto> GoalsTrackerUpdateItemAsync(UpdateItemRequest request)
        {
            var response = await PostAsync<GoalsTrackerItemDto>(ApiUrl.GoalsTrackerItemUpdate, request);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task GoalsTrackerDeleteItemAsync(long itemId)
        {
            await PostAsync<object>(ApiUrl.GoalsTrackerItemDelete, new DeleteItemRequest()
            {
                Id = itemId
            });
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
