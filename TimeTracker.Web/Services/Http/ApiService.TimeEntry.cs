using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Web.Core.Exceptions;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<TimeEntryDto> TimeEntryStartAsync(StartRequest model)
        {
            var response = await PostAuthorizedAsync<TimeEntryDto>(ApiUrl.TimeEntryStart, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<TimeEntryDto> TimeEntryStopAsync(StopRequest model)
        {
            var response = await PostAuthorizedAsync<TimeEntryDto>(ApiUrl.TimeEntryStop, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<TimeEntryDto> TimeEntrySetAsync(SetRequest model)
        {
            var response = await PostAuthorizedAsync<TimeEntryDto>(ApiUrl.TimeEntrySet, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<GetListResponse> TimeEntryGetListAsync(GetListRequest model)
        {
            var response = await PostAuthorizedAsync<GetListResponse>(ApiUrl.TimeEntryGetList, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task TimeEntryDeleteAsync(long id)
        {
            await PostAuthorizedAsync<TimeEntryDto>(ApiUrl.TimeEntryDelete, new DeleteRequest()
            {
                TimeEntryId = id
            });
        }
    }
}
