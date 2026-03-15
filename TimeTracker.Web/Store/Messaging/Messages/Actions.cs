using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Message;

namespace TimeTracker.Web.Store.Messaging.Messages;

public record struct AddMessageAction(MessagingMessageDto Message);
public record struct RefreshListAction();
public record struct LoadListAction(bool IsRefresh = true);
public record struct SetListAction(GetListResponse Response);
public record struct SetIsListLoadingAction(bool IsLoading);
public record struct SetIsMessageSending(bool IsSending);
public record struct SendMessageAction(string Text);
