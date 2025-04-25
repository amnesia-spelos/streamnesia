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
using TwitchLib.Communication.Models;

namespace Streamnesia.Twitch;

public class TwitchBot : ITwitchBot
{
    private readonly IConfigurationStorage _configStorage;
    private readonly ILogger<TwitchBot> _logger;

    private TwitchClient? _client;

    public event EventHandler<MessageEventArgs>? MessageReceived;

    public bool IsConnected => _client?.IsConnected ?? false;

    public TwitchBot(IConfigurationStorage configStorage, ILogger<TwitchBot> logger)
    {
        _configStorage = configStorage;
        _logger = logger;
    }

    private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        _logger.LogInformation("Twitch bot joined the chat");
        _client?.SendMessage(e.Channel, "imGlitch Streamnesia bot connected! Type the number of payload on screen you wish to vote for.");
    }
        
    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        => MessageReceived?.Invoke(this, new MessageEventArgs(e.ChatMessage.UserId, e.ChatMessage.Message));

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_client?.IsConnected ?? false)
        {
            _logger.LogWarning("Attempted to connect an already connected client.");
            return Task.FromResult(Result.Fail("The client is already connected."));
        }

        var config = _configStorage.ReadTwitchBotConfig();

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

            _client.Initialize(credentials, config.TwitchChannelName);
            _client.Connect();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception ocurred during Twitch client connection");
            return Task.FromResult(Result.Fail("An exception occurred during client connection"));
        }

        return Task.FromResult(Result.Ok());
    }

    public void Dispose()
    {
        if (_client is not null)
        {
            _client.OnJoinedChannel -= Client_OnJoinedChannel;
            _client.OnMessageReceived -= Client_OnMessageReceived;
            
            if (_client.IsConnected)
            {
                _client.Disconnect();
            }

            _client = null!;
        }

        GC.SuppressFinalize(this);
    }
}
