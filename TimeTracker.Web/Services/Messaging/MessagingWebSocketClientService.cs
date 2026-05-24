using Microsoft.AspNetCore.SignalR.Client;
using TimeTracker.Business.Extensions;

namespace TimeTracker.Web.Services.Messaging;

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
        Debug.Log($"Connecting to Hub: {_hubUrl}");
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect()
            .Build();

        InitEvents();
        
// Хуки на изменение состояния соединения (опционально)
        // hubConnection.Reconnecting += error =>
        // {
        //     lines.Add("⚠️ Потеряно соединение, переподключение...");
        //     InvokeAsync(StateHasChanged);
        //     return Task.CompletedTask;
        // };
        //
        // hubConnection.Reconnected += connectionId =>
        // {
        //     lines.Add("✅ Переподключение выполнено");
        //     InvokeAsync(StateHasChanged);
        //     return Task.CompletedTask;
        // };
        //
        _hubConnection.Closed += error =>
        {
            _isConnected = false;
            _logger.LogDebug($"❌ Connection to Hub {_hubUrl} closed");
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
            Debug.Log(ex);
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
