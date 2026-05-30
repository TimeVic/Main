using Microsoft.AspNetCore.SignalR.Client;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Client.Core.Services.Messaging;

public partial class MessagingWebSocketClientService: IDisposable
{
    private readonly ILogger<MessagingWebSocketClientService> _logger;
    private readonly IConfiguration _configuration;
    private HubConnection? _hubConnection;
    private readonly string _apiUrl;
    private bool _isConnected = false;
    private readonly string _hubUrl;

    public MessagingWebSocketClientService(
        ILogger<MessagingWebSocketClientService> logger,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _configuration = configuration;
        _apiUrl = (configuration.GetValue<string>("ApiUrl")!).EnsureTrailingSlash()!;
        _hubUrl = $"{_apiUrl}websocket/messaging";
    }

    public async Task Connect()
    {
        _logger.LogDebug("Connecting to Hub: {HubUrl}", _hubUrl);
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect()
            .Build();

        InitEvents();

        _hubConnection.Closed += error =>
        {
            _isConnected = false;
            _logger.LogDebug(error, "Connection to Hub {HubUrl} closed", _hubUrl);
            return Task.CompletedTask;
        };

        await StartAsync();
    }
    
    private async Task StartAsync()
    {
        try
        {
            await _hubConnection!.StartAsync();
            _isConnected = true;
            _logger.LogDebug($"✅ Connected to Hub: {_hubUrl}");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Connection to Hub {HubUrl} failed", _hubUrl);
            _isConnected = false;
        }
    }

    
    public async Task Send()
    {
        _logger.LogDebug($"Send message to Hub: {_hubUrl}");
        if (_hubConnection is null || !_isConnected)
            return;

        await _hubConnection.SendAsync("PingWithAuth");
    }
    
    public void Dispose()
    {
        _hubConnection?.DisposeAsync().GetAwaiter().GetResult();
        _hubConnection = null;
    }
}
