using Autofac;
using TimeTracker.Api.WebSocket.Core;

public partial class MessagingHub : BaseHub
{
    public MessagingHub(ILifetimeScope scope): base(scope)
    {
    }
}
