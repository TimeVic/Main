using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments;
namespace TimeTracker.Client.Core.Services.Http
{
    public partial class ApiService
    {
        public async Task<TaskCommentDto?> TaskCommentAddAsync(AddRequest model)
        {
            return await PostAsync<TaskCommentDto?>(ApiUrl.TaskCommentAdd, model);            
        }

        public async Task<TaskCommentDto?> TaskCommentUpdateAsync(UpdateRequest model)
        {
            return await PostAsync<TaskCommentDto?>(ApiUrl.TaskCommentUpdate, model);
        }
        
        public async Task TaskCommentDeleteAsync(Guid commentId)
        {
            await PostAsync<TaskCommentDto>(ApiUrl.TaskCommentDelete, new DeleteRequest() {
                CommentId = commentId
            });
        }

        public async Task<GetListResponse?> TaskCommentsGetListAsync(
            Guid taskId,
            int page
        )
        {
            return await PostAsync<GetListResponse?>(ApiUrl.TaskCommentsList, new GetListRequest()
            {
                TaskId = taskId,
                Page = page
            });
        }
    }
}
