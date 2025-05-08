using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using Streamnesia.Core;
using Streamnesia.Core.Conductors;
using Streamnesia.Core.Entities;

namespace Streamnesia.Execution.Conductors;

public class TwitchPollConductor(
    IAmnesiaClient amnesiaClient,
    IPayloadLoader payloadLoader,
    IPoll<ParsedPayload> poll,
    ITwitchBot twitchBot,
    ICommandQueue commandQueue,
    ILogger<TwitchPollConductor> logger
    ) : ITwitchPollConductor
{
    public event Func<CancellationToken, Task>? PollStartedAsync;

    private readonly ConcurrentDictionary<string, ParsedPayload?> _userPockets = new();
    private readonly ConcurrentDictionary<string, DateTime> _userPocketTimestamps = new();

    private bool _started = false;

    public bool IsRunning => _started;

    public async Task<Result> InitializeAsync()
    {
        if (_started)
        {
            logger.LogError("Twitch Poll Chaos already running");
            return Result.Fail("Twitch Poll Chaos already running");
        }
        if (!twitchBot.IsConnected)
        {
            logger.LogError("Twitch bot not connected");
            return Result.Fail("Twitch bot not connected");
        }

        if (!amnesiaClient.IsConnected)
        {
            logger.LogError("Amnesia Client not connected");
            return Result.Fail("Amnesia Client not connected");
        }

        if (payloadLoader.Payloads is null || payloadLoader.Payloads.Count < 4)
        {
            logger.LogError("Payloads are null or less than 4 payloads");
            return Result.Fail("Payloads are null or less than 4 payloads");
        }

        commandQueue.Start();
        logger.LogInformation("Command queue started...");

        _started = true;

        twitchBot.MessageReceived += OnTwitchMessage;

        if (PollStartedAsync is not null)
        {
            await PollStartedAsync.Invoke(CancellationToken.None);
        }

        BeginPollRound();
        return Result.Ok();
    }

    public Result Stop()
    {
        twitchBot.MessageReceived -= OnTwitchMessage;
        _started = false;

        logger.LogInformation("Twitch poll conductor stopped");
        return Result.Ok();
    }

    public Result<IReadOnlyCollection<ParsedPayload>> BeginPollRound()
    {
        logger.LogInformation("Starting a new round of voting");

        var allPayloads = payloadLoader.Payloads!;
        if (allPayloads.Count < 4)
        {
            logger.LogError("Less than 4 payloads available");
            return Result.Fail("Less than 4 payloads available");
        }

        var shuffled = allPayloads
            .OrderBy(_ => Guid.NewGuid())
            .Take(4)
            .ToList();

        poll.SetOptions(shuffled);

        return Result.Ok<IReadOnlyCollection<ParsedPayload>>(shuffled);
    }

    public Result ExecuteTopPayload()
    {
        if (!_started)
        {
            logger.LogError("Attempted execution in a stopped poll");
            return Result.Fail("Attempted execution in a stopped poll");
        }

        var payload = poll.GetVotedItem();

        if (!amnesiaClient.IsConnected)
        {
            logger.LogError("Amnesia client is not connected");
            return Result.Fail("Amnesia client is not connected");
        }

        commandQueue.AddPayload(payload);

        return Result.Ok();
    }

    private void OnTwitchMessage(object? sender, MessageEventArgs e)
    {
        if (!_started)
        {
            logger.LogError("Received Twitch message in a stopped poll");
            return;
        }

        if (e.Message.StartsWith("!pocket"))
        {
            logger.LogDebug("Received a pocket message: {message}", e.Message);
            ProcessPocketMessage(e.UserId, e.Message);

            return;
        }

        var success = int.TryParse(e.Message, out var index);

        if (!success)
        {
            logger.LogDebug("Ignored non-numeric vote: {Message}", e.Message);
            return;
        }

        var result = poll.SetNamedVote(e.UserId, index);

        if (result.IsFailed)
        {
            logger.LogError("Setting a vote failed: {Result}", string.Join(", ", result.Errors.Select(e => e.Message)));
            return;
        }

        logger.LogInformation("Processed vote from {UserId} to index {Index}", e.UserId, index);
    }

    public Result<IReadOnlyDictionary<ParsedPayload, int>> GetCurrentPollState()
    {
        if (!_started)
        {
            logger.LogError("Cannot return state because poll hasn't started");
            return Result.Fail("Cannot return state because poll hasn't started");
        }

        if (poll.Options is null || poll.Options.Count == 0)
        {
            logger.LogError("Cannot return state because poll options are null or empty");
            return Result.Fail("Cannot return state because poll options are null or empty");
        }

        var state = poll.Options
            .Select((payload, index) => new
            {
                Key = payload,
                Value = poll.GetVotesByOptionIndex(index)
            })
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        return Result.Ok<IReadOnlyDictionary<ParsedPayload, int>>(state);
    }

    private void ProcessPocketMessage(string user, string message)
    {
        if (message.Trim() == "!pocket")
        {
            logger.LogDebug("Pop pocket requested");

            if (!_userPockets.TryGetValue(user, out ParsedPayload? payload) || payload is null)
            {
                logger.LogDebug("User does not have anything in their pocket");
                return;
            }

            if (payload is not null)
            {
                commandQueue.AddPayload(payload);
                _userPockets[user] = null;
                twitchBot.SendMessage("Pocket payload executed.");
            }

            return;
        }

        var split = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2 || !int.TryParse(split[1], out var payloadIndex))
        {
            twitchBot.SendMessage("Pocket Usage: !pocket <payload_vote_number>");
            return;
        }

        if (payloadIndex < 0 || payloadIndex > 3)
        {
            logger.LogDebug("Pocket payload index out of range.");
            return;
        }

        if (_userPocketTimestamps.TryGetValue(user, out var timestamp) && (DateTime.Now - timestamp).TotalMinutes < 5) // TODO: Configurable option + disable pockets
        {
            logger.LogDebug("Pocket cooldown");
            twitchBot.SendMessage("You can only pocket once every 5 minutes");
            return;
        }

        var payloadToPocket = poll.Options?.ElementAt(payloadIndex);
        if (payloadToPocket is not null)
        {
            _userPocketTimestamps[user] = DateTime.Now;
            _userPockets[user] = payloadToPocket;
            twitchBot.SendMessage($"Pocketed '{payloadToPocket.Name}'. Use '!pocket' to use it");
            logger.LogDebug("Item pocketed: {Name}", payloadToPocket.Name);
        }
    }
}
