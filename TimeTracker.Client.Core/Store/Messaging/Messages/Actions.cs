using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Message;

namespace TimeTracker.Client.Core.Store.Messaging.Messages;

public record struct AddMessageAction(MessagingMessageDto Message);
public record struct RefreshListAction(MessagingChannelDto Channel);
public record struct SetPageAction(MessagingChannelDto Channel, int Page);
public record struct LoadListAction(MessagingChannelDto Channel, bool IsRefresh = true);
public record struct SetListAction(MessagingChannelDto Channel, GetListResponse Response);
public record struct SetIsListLoadingAction(bool IsLoading);
public record struct SetIsMessageSending(bool IsSending);
public record struct SendMessageAction(string Text);
