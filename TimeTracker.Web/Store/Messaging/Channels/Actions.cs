using TimeTracker.Api.Shared.Dto.Entity.Messaging;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Messaging.Channel;

namespace TimeTracker.Web.Store.Messaging.Channels;

public record struct AddChannelAction(MessagingChannelDto Channel);
public record struct SetSelectedAction(MessagingChannelDto? Channel);
public record struct RefreshListAction();
public record struct LoadListAction(bool IsRefresh = true);
public record struct SetListAction(GetListResponse Response);
public record struct SetIsListLoadingAction(bool IsLoading);
