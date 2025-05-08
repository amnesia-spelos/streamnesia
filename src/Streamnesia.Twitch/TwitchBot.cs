using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using Streamnesia.Core;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace Streamnesia.Twitch;

public class TwitchBot(
    IConfigurationStorage cfgStorage,
    ILogger<TwitchBot> logger) : ITwitchBot
{

    private TwitchClient? _client;
    private string _errorMessage = string.Empty;
    private string _channel = string.Empty;

    public event EventHandler<MessageEventArgs>? MessageReceived;
    public event AsyncTwitchBotStateChangedHandler? StateChangedAsync;

    public bool IsConnected => _client?.IsConnected ?? false;

    private TwitchBotState _state;
    public TwitchBotState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                _ = RaiseStateChangedAsync(_state);
            }
        }
    }

    private async Task RaiseStateChangedAsync(TwitchBotState newState)
    {
        if (StateChangedAsync == null) return;

        foreach (var handler in StateChangedAsync.GetInvocationList())
        {
            try
            {
                await ((AsyncTwitchBotStateChangedHandler)handler)(this, newState, _errorMessage);
                _errorMessage = string.Empty;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to raise state changed event");
            }
        }
    }

    private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        State = TwitchBotState.Connected;
        logger.LogInformation("Twitch bot joined the chat");
        _client?.SendMessage(e.Channel, "imGlitch Streamnesia bot connected! Type the number of payload on screen you wish to vote for.");
        _channel = e.Channel;
    }
        
    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        => MessageReceived?.Invoke(this, new MessageEventArgs(e.ChatMessage.UserId, e.ChatMessage.Message));

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_client?.IsConnected ?? false)
        {
            logger.LogWarning("Attempted to connect an already connected client.");
            return Task.FromResult(Result.Fail("The client is already connected."));
        }

        _errorMessage = string.Empty;
        State = TwitchBotState.Connecting;

        var config = cfgStorage.ReadTwitchBotConfig();

        try
        {
            var credentials = new ConnectionCredentials(config.BotName, config.BotApiKey);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            var customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
            
            _client.OnJoinedChannel += Client_OnJoinedChannel;
            _client.OnMessageReceived += Client_OnMessageReceived;
            _client.OnError += Client_OnError;
            _client.OnConnectionError += Client_OnConnectionError;
            _client.OnFailureToReceiveJoinConfirmation += Client_OnFailureToReceiveJoinConfirmation;

            _client.Initialize(credentials, config.TwitchChannelName);
            _client.Connect();
        }
        catch (Exception e)
        {
            _errorMessage = "Failed to connect, ensure twitch bot is configured in the settings";
            State = TwitchBotState.Failed;
            logger.LogError(e, "An exception ocurred during Twitch client connection");
            return Task.FromResult(Result.Fail("An exception occurred during client connection"));
        }

        return Task.FromResult(Result.Ok());
    }

    private void Client_OnFailureToReceiveJoinConfirmation(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
    {
        logger.LogError("Failed to join: {Details}", e.Exception.Details);

        Stop(stateChange: false);

        _errorMessage = "Failed to join chat. Is your bot token correct?";
        State = TwitchBotState.Failed;
    }

    private void Client_OnConnectionError(object? sender, OnConnectionErrorArgs e)
    {
        logger.LogError("Connection error: {Message}", e.Error.Message);
        _errorMessage = $"Connection error: {e.Error.Message}";
        State = TwitchBotState.Failed;
    }

    public void Dispose()
    {
        if (_client is not null)
            _client = null!;

        GC.SuppressFinalize(this);
    }

    private void Client_OnError(object? sender, OnErrorEventArgs e)
    {
        logger.LogError(e.Exception, "A twitch bot error occurred");
        _errorMessage = "Twitch bot errored";
        State = TwitchBotState.Failed;
    }

    public void Stop(bool stateChange = true)
    {
        if (_client is null || !_client.IsConnected)
        {
            logger.LogWarning("Cannot stop a disconnected client");
            return;
        }

        _client.OnJoinedChannel -= Client_OnJoinedChannel;
        _client.OnMessageReceived -= Client_OnMessageReceived;
        _client.OnError -= Client_OnError;
        _client.OnConnectionError -= Client_OnConnectionError;

        if (_client.IsConnected)
        {
            _client.Disconnect();
        }

        _client = null!;

        if (stateChange)
        {
            _errorMessage = string.Empty;
            State = TwitchBotState.Disconnected;
            logger.LogInformation("Twitch client disconnected");
        }
    }

    public void SendMessage(string message)
    {
        _client?.SendMessage(_channel, message);
    }
}
