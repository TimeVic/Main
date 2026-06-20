using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Common.Dto;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;
using ClientAddRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client.AddRequest;
using ClientGetListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client.GetListRequest;
using ClientGetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client.GetListResponse;
using ClientPaymentAddRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment.AddRequest;
using ClientPaymentGetListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment.GetListRequest;
using ClientPaymentGetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment.GetListResponse;
using ClientPaymentUpdateRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.ClientPayment.UpdateRequest;
using ClientUpdateRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Client.UpdateRequest;
using GoalsTrackerUpdateItemRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.GoalsTracker.UpdateItemRequest;
using MemberPaymentAddRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment.AddRequest;
using MemberPaymentGetListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment.GetListRequest;
using MemberPaymentGetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment.GetListResponse;
using MemberPaymentUpdateRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.MemberPayment.UpdateRequest;
using MessagingChannelGetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel.GetListResponse;
using MessagingMessageGetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Message.GetListResponse;
using NotificationCenterGetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.NotificationsCenter.GetListResponse;
using ProjectAddRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project.AddRequest;
using ProjectGetListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project.GetListRequest;
using ProjectGetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project.GetListResponse;
using ProjectUpdateRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project.UpdateRequest;
using TagAddRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag.AddRequest;
using TagGetListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag.GetListRequest;
using TagGetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag.GetListResponse;
using TagUpdateRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag.UpdateRequest;
using TaskListGetListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List.GetListRequest;
using TaskListGetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List.GetListResponse;
using TasksGetListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.GetListRequest;
using TasksGetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.GetListResponse;
using TasksUpdatePositionsRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.UpdatePositionsRequest;
using TasksUpdateRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.UpdateRequest;
using TimeEntryGetFilteredListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.GetFilteredListResponse;
using TimeEntryGetFilteredListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.GetFilteredListRequest;
using TimeEntryGetListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.GetListRequest;
using TimeEntryGetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.GetListResponse;
using TimeEntrySetRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.SetRequest;
using TimeEntryStartRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.StartRequest;
using TimeEntryStopRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.StopRequest;
using WorkspaceMemberGetListRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember.GetListRequest;
using WorkspaceMemberGetListResponse = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember.GetListResponse;
using WorkspaceMemberUpdateRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember.UpdateRequest;
using WorkspaceUpdateRequest = TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace.UpdateRequest;

namespace TimeTracker.Client.Core.Services.Http;

public interface IApiService
{
    Task<bool> CheckIsLoggedInAsync();

    Task<UserDto?> UserGetCurrentAsync();

    Task LogoutAsync();

    Task<UserDto?> UserSelectWorkspaceAsync(Guid workspaceId);

    Task<ClientDto?> ClientAddAsync(ClientAddRequest model);

    Task<ClientDto> ClientUpdateAsync(ClientUpdateRequest model);

    Task<ClientGetListResponse?> ClientGetListAsync(ClientGetListRequest model);

    Task<ClientPaymentDto?> ClientPaymentAddAsync(ClientPaymentAddRequest model);

    Task<ClientPaymentDto?> ClientPaymentUpdateAsync(ClientPaymentUpdateRequest model);

    Task ClientPaymentDeleteAsync(Guid paymentId);

    Task<ClientPaymentGetListResponse?> ClientPaymentGetListAsync(ClientPaymentGetListRequest model);

    Task<GoalsTrackerDto?> GoalsTrackerLoadAsync(Guid workspaceId, DateTime date);

    Task GoalsTrackerChangePositionsAsync(Guid workspaceId, DateTime date, ICollection<GoalsTrackerItemDto> items);

    Task<GoalsTrackerItemDto?> GoalsTrackerCreateItemAsync(Guid trackerId, string name, int numberOfTimes);

    Task<GoalsTrackerItemDto?> GoalsTrackerUpdateItemAsync(GoalsTrackerUpdateItemRequest request);

    Task GoalsTrackerDeleteItemAsync(Guid itemId);

    Task<GoalsTrackerCompletionMarkerDto?> GoalsTrackerSetCompletionAsync(Guid itemId, int dayOfMonth, bool isChecked);

    Task<ListResponse<CurrencyDto>> ListCurrenciesGetAll();

    Task<MemberPaymentDto?> MemberPaymentAddAsync(MemberPaymentAddRequest model);

    Task<MemberPaymentDto?> MemberPaymentUpdateAsync(MemberPaymentUpdateRequest model);

    Task MemberPaymentDeleteAsync(Guid paymentId);

    Task<MemberPaymentGetListResponse?> MemberPaymentGetListAsync(MemberPaymentGetListRequest model);

    Task<MessagingChannelDto?> MessagingChannelCreateAsync(Guid workspaceId, string slug);

    Task<MessagingChannelDto?> MessagingChannelInitAsync(Guid workspaceId);

    Task<MessagingChannelGetListResponse?> MessagingChannelGetListAsync(Guid workspaceId);

    Task MessagingMessageSendAsync(Guid workspaceId, string text, Guid? receiverId = null, Guid? channelId = null);

    Task<MessagingMessageGetListResponse?> MessagingMessageGetListAsync(Guid channelId, int page);

    Task<int> NotificationCenterGetUnreadCount(Guid workspaceId);

    Task<NotificationCenterGetListResponse?> NotificationCenterGetList(Guid workspaceId, int page);

    Task NotificationCenterMarkAllAsRead(Guid workspaceId);

    Task NotificationCenterMarkAsRead(Guid notificationId);

    Task<GetNotesTreeResponse?> NotesGetTreeAsync(GetNotesTreeRequest model);

    Task<NoteDocumentDto?> NotesGetDocumentAsync(GetNoteDocumentRequest model);

    Task<NoteTreeNodeDto?> NotesCreateFolderAsync(CreateNoteFolderRequest model);

    Task<NoteDocumentDto?> NotesCreateDocumentAsync(CreateNoteDocumentRequest model);

    Task<NoteDocumentDto?> NotesUpdateDocumentAsync(UpdateNoteDocumentRequest model);

    Task<NoteTreeNodeDto?> NotesRenameNodeAsync(RenameNoteNodeRequest model);

    Task<NoteTreeNodeDto?> NotesMoveNodeAsync(MoveNoteNodeRequest model);

    Task<NoteTreeNodeDto?> NotesArchiveNodeAsync(ArchiveNoteNodeRequest model);

    Task<GetLinkedNotesResponse?> NotesGetLinkedNotesAsync(GetLinkedNotesRequest model);

    Task<NoteLinkDto?> NotesCreateLinkAsync(CreateNoteLinkRequest model);

    Task NotesDeleteLinkAsync(DeleteNoteLinkRequest model);

    Task<ProjectDto?> ProjectAddAsync(ProjectAddRequest model);

    Task<ProjectDto?> ProjectUpdateAsync(ProjectUpdateRequest model);

    Task<ProjectGetListResponse?> ProjectGetListAsync(ProjectGetListRequest model);

    Task<MemberPaymentReportResponse?> ReportsGetMemberPaymentsReportAsync(Guid workspaceId, DateTime endDate);

    Task<SummaryReportResponse?> ReportsGetSummaryReportAsync(
        Guid workspaceId,
        DateTime startDate,
        DateTime endTime,
        SummaryReportType reportType
    );

    Task<WorkspaceFinancialSummaryReportResponse?> ReportsGetWorkspaceFinancialSummaryAsync(Guid workspaceId);

    Task<TagDto?> TagAddAsync(TagAddRequest model);

    Task<TagDto?> TagUpdateAsync(TagUpdateRequest model);

    Task TagDeleteAsync(Guid tagId);

    Task<TagGetListResponse?> TagGetListAsync(TagGetListRequest model);

    Task<TaskFullDto?> TasksUpdateAsync(TasksUpdateRequest model);

    Task TasksUpdatePositionsAsync(TasksUpdatePositionsRequest request);

    Task<TasksGetListResponse?> TasksGetListAsync(TasksGetListRequest model);

    Task<TasksGetListResponse?> TasksGetOverdueListAsync(
        Guid workspaceId,
        string? searchString = null
    );

    Task TaskListArchiveAsync(Guid taskListId);

    Task<TaskListGetListResponse?> TaskListGetListAsync(TaskListGetListRequest model);

    Task<TimeEntryDto?> TimeEntryStartAsync(TimeEntryStartRequest model);

    Task<TimeEntryDto?> TimeEntryStopAsync(TimeEntryStopRequest model);

    Task<TimeEntryDto?> TimeEntrySetAsync(TimeEntrySetRequest model);

    Task<TimeEntryGetListResponse?> TimeEntryGetListAsync(TimeEntryGetListRequest model);

    Task<TimeEntryGetFilteredListResponse?> TimeEntryGetFilteredListAsync(TimeEntryGetFilteredListRequest model);

    Task TimeEntryDeleteAsync(Guid id);

    Task<PaginatedListDto<WorkspaceDto>?> WorkspaceGetListAsync();

    Task<WorkspaceDto?> WorkspaceUpdateAsync(WorkspaceUpdateRequest model);

    Task<WorkspaceMemberDto?> WorkspaceMemberAddAsync(Guid workspaceId, string email);

    Task<WorkspaceMemberDto?> WorkspaceMemberUpdateAsync(WorkspaceMemberUpdateRequest request);

    Task<WorkspaceMemberGetListResponse?> WorkspaceMemberGetListAsync(WorkspaceMemberGetListRequest model);

    Task WorkspaceMemberDeleteAsync(Guid memberId);
}
