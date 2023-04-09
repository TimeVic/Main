using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles;

public class StorageProfile : Profile
{
    public StorageProfile()
    {
        CreateMap<StoredFileEntity, StoredFileDto>();
    }
}
