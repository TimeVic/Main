using Domain.Abstractions;
using MongoDB.Bson;

namespace TimeTracker.Business.FileStorage.Services;

public interface IMongoClient: IDomainService
{
    Task<ObjectId> UploadFileFromStream(
        string bucketName,
        string directory,
        string fileName,
        Stream fileStream
    );

    Task<Stream> DownloadToStream(
        string bucketName,
        string directory,
        string fileName
    );
}
