using AutoMapper;
using TimeTracker.Api.FileStorage.Dto.Entities;
using TimeTracker.Business.Orm.Entities.FileStorage;

namespace TimeTracker.Api.FileStorage.Profiles;

public class FileStorageFileProfile : Profile
{
    public FileStorageFileProfile()
    {
        CreateMap<FileStorageFileEntity, FileStorageFileDto>()
            .ForMember(
                item => item.Id,
                mapper => mapper.MapFrom(
                    item => item.ExternalId
                )
            )
            .ForMember(
                item => item.BucketName,
                mapper => mapper.MapFrom(
                    item => item.Bucket.Name
                )
            )
            .ForMember(
                item => item.FileName,
                mapper => mapper.MapFrom(
                    item => item.OriginalFileName
                )
            )
            .ForMember(
                item => item.Directory,
                mapper => mapper.MapFrom(
                    item => item.Directory != null ? item.Directory.FullPath : null
                )
            );
    }
}
