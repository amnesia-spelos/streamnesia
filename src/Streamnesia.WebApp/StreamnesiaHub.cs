using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Streamnesia.Core;
using Streamnesia.Payloads;
using Streamnesia.Twitch;
using Streamnesia.WebApp.Hubs;

namespace Streamnesia.WebApp
{
    public class StreamnesiaHub
    {
        private readonly IHubContext<UpdateHub> _hub;
        private readonly ICommandPoll _poll;
        private readonly CommandQueue _cmdQueue;
        private readonly IPayloadLoader _payloadLoader;
        private readonly TwitchBot _bot;
        private readonly Random _rng;
        private readonly PollState _pollState;
        private readonly ILogger<StreamnesiaHub> _logger;

        private IEnumerable<Payload> _payloads;
        private bool _canQueryPoll;

        public StreamnesiaHub(
            IHubContext<UpdateHub> hub,
            ICommandPoll poll,
            CommandQueue cmdQueue,
            IPayloadLoader payloadLoader,
            TwitchBot bot,
            Random rng,
            PollState pollState,
            StreamnesiaConfig config,
            ILogger<StreamnesiaHub> logger
        )
        {
            _hub = hub;
            _poll = poll;
            _cmdQueue = cmdQueue;
            _payloadLoader = payloadLoader;
            _bot = bot;
            _rng = rng;
            _pollState = pollState;
            _logger = logger;

            _canQueryPoll = true;

            _bot.OnVoted = OnUserVoted;
            _bot.OnDeathSet = async text => {
                if(config.AllowDeathMessages)
                    await _cmdQueue.Amnesia.SetDeathHintTextAsync(text);
            };
            _bot.OnMessageSent = async text => {
                if(config.AllowOnScreenMessages)
                    await _cmdQueue.Amnesia.DisplayTextAsync(text);
            };

            _ = _cmdQueue.StartCommandProcessingAsync(CancellationToken.None);
            _ = StartLoop();
        }

        public async Task StartLoop()
        {
            _logger.LogInformation("Starting the loop");

            await _cmdQueue.Amnesia.AttachToGameAsync();
            _logger.LogInformation("Attached to game");

            _payloads = await _payloadLoader.GetPayloadsAsync();
            _poll.SetPayloadSource(_payloads);

            while(true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                await UpdatePollAsync();
            }
        }

        private async Task UpdatePollAsync()
        {
            _pollState.StepForward();

            if(_pollState.Cooldown)
                return;

            if(_pollState.ShouldRegenerate)
            {
                _canQueryPoll = false;
                _poll.GenerateNew();
                _canQueryPoll = true;
            }

            var progressPercentage = _pollState.GetProgressPercentage();

            if(progressPercentage < 100.0)
            {
                await SendCurrentStatusAsync(progressPercentage, _poll.GetPollOptions(), _pollState.IsRapidfire);
            }
            else
            {
                await SendClearStatusAsync();

                if(!_pollState.IsRapidfire)
                {
                    var payload = _poll.GetPayloadWithMostVotes();
                    _cmdQueue.AddPayload(payload);
                }

                _pollState.Cooldown = true;
            }
        }

        private void OnUserVoted(string displayname, int vote)
        {
            if (!_canQueryPoll)
                return;

            vote--; // From label to index

            if(vote < 0)
                return;

            if(_pollState.IsRapidfire)
            {
                try
                {
                    var payload = _poll.GetPayloadAt(vote);
                    _cmdQueue.AddPayload(payload);
                    return;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            _poll.Vote(displayname, vote);
        }

        private async Task SendCurrentStatusAsync(double percentage, IEnumerable<PollOption> options, bool isRapidMode)
        {
            _logger.LogInformation($"Sending {percentage}% update");
            await _hub.Clients.All.SendCoreAsync("UpdateTimePercentage", new object[] { new {
                percentage,
                options = options.Select(p => new {
                    name = p.Name,
                    votes = p.Votes,
                    description = $"Send <code class='code-pop'>{p.Index + 1}</code> in the chat to vote for:"
                }),
                rapidFire = isRapidMode
            } });
        }

        private async Task SendClearStatusAsync()
        {
            _logger.LogInformation($"Sending clear update");
            await _hub.Clients.All.SendCoreAsync("UpdateTimePercentage", new object[] { new {
                percentage = 100.0,
                options = new object[0],
                rapidFire = false
            } });
        }
    }
}
