﻿using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Api.Profiles;

public class WorkspaceMemberProfile : Profile
{
    public WorkspaceMemberProfile()
    {
        CreateMap<WorkspaceMembershipEntity, WorkspaceMembershipDto>()
            .ForMember(
                dest => dest.Projects,
                opt => opt.MapFrom(
                    src => src.ProjectAccesses.Select(
                        item => item.Project
                    )
                )
            );
    }
}