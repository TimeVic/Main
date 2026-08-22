using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Api.Profiles.Api.Workspace;

public class WorkspaceProfile : Profile
{
    public WorkspaceProfile()
    {
        CreateMap<WorkspaceEntity, WorkspaceDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                return new WorkspaceDto
                {
                    Id = src.Id,
                    Name = src.Name,
                    IsDefault = src.IsDefault,
                    Description = src.Description,
                    TimeZone = src.TimeZone,
                    Mode = src.Mode,
                    IsApprovalsEnabled = src.IsApprovalsEnabled,
                    Currency = mapper.Mapper.Map<CurrencyDto>(src.Currency),
                };
            });
    }
}
