using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker;

namespace TimeTracker.Client.Core.Services.Http
{
    public partial class ApiService
    {
        public async Task<GoalsTrackerDto?> GoalsTrackerLoadAsync(Guid workspaceId, DateTime date)
        {
            return await PostAsync<GoalsTrackerDto?>(ApiUrl.GoalsTrackerGet, new GetRequest()
            {
                Date = date,
            });
        }
        
        public async Task GoalsTrackerChangePositionsAsync(
            Guid workspaceId,
            DateTime date,
            ICollection<GoalsTrackerItemDto> items
        )
        {
            await PostAsync<object>(ApiUrl.GoalsTrackerChangePositions, new ChangePositionsRequest()
            {
                Date = date,
                Positions = items.ToDictionary(x => x.Id, y => y.Position)
            });
        }
        
        public async Task<GoalsTrackerItemDto?> GoalsTrackerCreateItemAsync(Guid trackerId, string name, int numberOfTimes)
        {
            return await PostAsync<GoalsTrackerItemDto?>(ApiUrl.GoalsTrackerItemCreate, new CreateItemRequest()
            {
                GoalsTrackerId = trackerId,
                NumberOfTimes = numberOfTimes,
                Name = name
            });
        }
        
        public async Task<GoalsTrackerItemDto?> GoalsTrackerUpdateItemAsync(UpdateItemRequest request)
        {
            return await PostAsync<GoalsTrackerItemDto?>(ApiUrl.GoalsTrackerItemUpdate, request);
        }
        
        public async Task GoalsTrackerDeleteItemAsync(Guid itemId)
        {
            await PostAsync<object>(ApiUrl.GoalsTrackerItemDelete, new DeleteItemRequest()
            {
                Id = itemId
            });
        }
        
        public async Task<GoalsTrackerCompletionMarkerDto?> GoalsTrackerSetCompletionAsync(Guid itemId, int dayOfMonth, bool isChecked)
        {
            return await PostAsync<GoalsTrackerCompletionMarkerDto?>(ApiUrl.GoalsTrackerItemSetCompletion, new SetCompletionRequest()
            {
                GoalsTrackerItemId = itemId,
                DayOfMonth = dayOfMonth,
                IsChecked = isChecked
            });
        }
    }
}
