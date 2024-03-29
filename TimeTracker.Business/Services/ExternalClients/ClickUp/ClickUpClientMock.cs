﻿using System.Globalization;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.ExternalClients.ClickUp.Model;
using TimeTracker.Business.Services.ExternalClients.Dto;

namespace TimeTracker.Business.Services.ExternalClients.ClickUp;

public class ClickUpClientMock: IClickUpClient
{
    public ICollection<TimeEntryEntity> SentTimeEntries = new List<TimeEntryEntity>();
    
    public bool IsSent => SentTimeEntries.Count > 0;

    public void Reset()
    {
        SentTimeEntries.Clear();
    }

    public async Task<SynchronizedTimeEntryDto?> SetTimeEntryAsync(TimeEntryEntity timeEntry)
    {
        SentTimeEntries.Add(timeEntry);
        return new SynchronizedTimeEntryDto()
        {
            Id = "123",
            AdditionalDescription = "Test description"
        };
    }

    public Task<bool> IsFillTimeEntryDescriptionFromTaskTitle(TimeEntryEntity timeEntry)
    {
        return Task.FromResult(true);
    }

    public async Task<GetTaskResponseDto?> GetTaskAsync(TimeEntryEntity timeEntry)
    {
        return null;
    }

    public bool IsCorrectTaskId(TimeEntryEntity timeEntry)
    {
        return true;
    }

    public Task<bool> DeleteTimeEntryAsync(TimeEntryEntity timeEntry)
    {
        SentTimeEntries.Add(timeEntry);
        return Task.FromResult(true);
    }

    public Task<bool> IsValidClientSettings(WorkspaceEntity workspace, UserEntity user)
    {
        return Task.FromResult<bool>(true);
    }
}
