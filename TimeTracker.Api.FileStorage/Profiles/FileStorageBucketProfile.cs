using AutoMapper;
using TimeTracker.Api.FileStorage.Dto.Entities;
using TimeTracker.Business.Orm.Entities.FileStorage;

namespace TimeTracker.Api.FileStorage.Profiles;

public class FileStorageBucketProfile : Profile
{
    public FileStorageBucketProfile()
    {
        CreateMap<FileStorageBucketEntity, FileStorageFileDto>();
    }
}
