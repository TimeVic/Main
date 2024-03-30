using Domain.Abstractions;
using MongoDB.Bson;

namespace TimeTracker.Business.FileStorage.Services;

public interface IMongoClient: IDomainService
{
    Task<ObjectId> UploadFileFromStream(
        string usersBucketName,
        string directory,
        string fileName,
        Stream fileStream
    );

    Task<Stream> DownloadToStream(
        string usersBucketName,
        string directory,
        string fileName
    );
}
