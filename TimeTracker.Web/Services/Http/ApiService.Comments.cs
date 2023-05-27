using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments;
using TimeTracker.Web.Core.Exceptions;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<TaskCommentDto> TaskCommentAddAsync(AddRequest model)
        {
            var response = await PostAuthorizedAsync<TaskCommentDto>(ApiUrl.TaskCommentAdd, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }

        public async Task<TaskCommentDto> TaskCommentUpdateAsync(UpdateRequest model)
        {
            var response = await PostAuthorizedAsync<TaskCommentDto>(ApiUrl.TaskCommentUpdate, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task TaskCommentDeleteAsync(long commentId)
        {
            await PostAuthorizedAsync<TaskCommentDto>(ApiUrl.TaskCommentDelete, new DeleteRequest() {
                CommentId = commentId
            });
        }

        public async Task<GetListResponse> TaskCommentsGetListAsync(long taskId, int page)
        {
            var response = await PostAuthorizedAsync<GetListResponse>(ApiUrl.TaskCommentsList, new GetListRequest()
            {
                TaskId = taskId,
                Page = page
            });
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
    }
}
