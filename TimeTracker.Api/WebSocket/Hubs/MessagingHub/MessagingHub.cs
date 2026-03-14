using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TimeTracker.Api.WebSocket.Constants;
using TimeTracker.Api.WebSocket.Core;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Messaging;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using IAuthorizationService = TimeTracker.Business.Services.Auth.IAuthorizationService;

public class MessagingHub : BaseHub
{
    public MessagingHub(ILifetimeScope scope): base(scope)
    {
    }
}
