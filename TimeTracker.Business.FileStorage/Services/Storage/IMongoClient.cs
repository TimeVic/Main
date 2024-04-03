using Domain.Abstractions;
using MongoDB.Bson;

namespace TimeTracker.Business.FileStorage.Services.Storage;

public interface IMongoClient: IDomainService
{
    Task<ObjectId> UploadFileFromStream(
        string usersBucketName,
        string fileName,
        Stream fileStream
    );

    Task<Stream> DownloadToStream(
        string usersBucketName,
        string fileName
    );
}
