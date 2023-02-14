﻿using Domain.Abstractions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Storage;

public partial class FileStorage: IFileStorage
{
    public async Task<(StoredFileEntity, Stream)> GetFileStream(UserEntity user, long fileId)
    {
        var file = await _dbSessionProvider.CurrentSession.GetAsync<StoredFileEntity>(fileId);
        if (file == null)
        {
            throw new RecordNotFoundException();
        }
        if (!await _securityManager.HasAccess(AccessLevel.Read, user, file.Relationship))
        {
            throw new HasNoAccessException();
        }

        var fileStream = new MemoryStream();
        await _googleClient.DownloadObjectAsync(_bucketName, file.CloudFilePath, fileStream);
        return (file, fileStream);
    }
}
