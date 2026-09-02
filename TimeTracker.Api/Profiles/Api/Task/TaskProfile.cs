using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Api.Utils;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dto.Tasks;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Api.Profiles.Api.Task;

public class TaskProfile : Profile
{
    public TaskProfile()
    {
        CreateMap<TaskEntity, TaskDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                var taskList = mapper.Mapper.Map<TaskListDto>(src.TaskList);
                var tags = mapper.Mapper.Map<ICollection<TagDto>>(src.Tags.ToArray());
                var user = mapper.Mapper.Map<UserDto>(src.User);
                return new TaskDto
                {
                    Id = src.Id,
                    PositionIndex = src.PositionIndex,
                    TaskId = src.TaskId,
                    Status = src.Status,
                    ExtendedStatus = src.ExtendedStatus,
                    Priority = src.Priority,
                    Title = src.Title,
                    Description = src.Description,
                    ExternalTaskId = src.ExternalTaskId,
                    OriginalEstimate = src.OriginalEstimate,
                    ExternalSourceType = src.ExternalSourceType,
                    StartTime = src.StartTime,
                    EndTime = src.EndTime,
                    ReminderTime = src.ReminderTime,
                    IsArchived = src.IsArchived,
                    UpdatedAt = src.UpdatedAt,
                    CreatedAt = src.CreatedAt,
                    TaskList = taskList,
                    Tags = tags,
                    User = user
                };
            });
            
        CreateMap<TaskEntity, TaskFullDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                return MappingUtils.BuildWithBase<
                    TaskEntity,
                    TaskDto,
                    TaskFullDto
                >(
                    src,
                    mapper.Mapper,
                    src =>
                    {
                        var user = mapper.Mapper.Map<UserDto>(src.User);
                        var tags = mapper.Mapper.Map<List<TagDto>>(src.Tags.ToList());
                        var attachments = mapper.Mapper.Map<List<StoredFileDto>>(src.Attachments.ToList());
                        return new TaskFullDto
                        {
                            User = user,
                            Attachments = attachments,
                            Tags = tags
                        };
                    }
                );
            });
        
        CreateMap<UpdateRequest, TaskEntity>()
            .ForMember(
                dto => dto.TaskId,
                builder => builder.Ignore()
            );
        CreateMap<GetListFilterRequest, GetTasksFilterDto>();
    }
}
