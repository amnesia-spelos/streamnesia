using System;
using Streamnesia.CommandProcessing;
using Streamnesia.Payloads;

namespace Streamnesia.WebApp
{
    public class PollState
    {
        private double _cooldownDurationInSeconds = 5.0;
        private double _rapidfireDurationInSeconds = 20.0;
        private double _votingDurationInSeconds = 40.0;
        private int _rapidfireChancePrecent = 10;

        private readonly Random _rng;

        public bool Cooldown { get; set; } = true;
        public bool ShouldRegenerate { get; private set; }
        public bool IsRapidfire { get; private set; }
        private DateTime? _cooldownEnd;
        private DateTime _pollStartDateTime = DateTime.Now;
        private DateTime _pollEndDateTime = DateTime.Now;

        public PollState(Random rng, StreamnesiaConfig config)
        {
            _rng = rng;
            _cooldownDurationInSeconds = config.CooldownDurationInSeconds;
            _rapidfireDurationInSeconds = config.RapidfireDurationInSeconds;
            _votingDurationInSeconds = config.VotingDurationInSeconds;
            _rapidfireChancePrecent = config.RapidFireChancePercent;
        }

        internal void StepForward()
        {
            ShouldRegenerate = false;
            
            if(Cooldown)
            {
                if(!_cooldownEnd.HasValue)
                    _cooldownEnd = DateTime.Now.Add(TimeSpan.FromSeconds(_cooldownDurationInSeconds));

                if(DateTime.Now < _cooldownEnd)
                    return;

                _cooldownEnd = null;
                Cooldown = false;
                _pollStartDateTime = DateTime.Now;


                if (ShouldStartRapidfire())
                {
                    IsRapidfire = true;
                    _pollEndDateTime = DateTime.Now.Add(TimeSpan.FromSeconds(_rapidfireDurationInSeconds));
                }
                else
                {
                    IsRapidfire = false;
                    _pollEndDateTime = DateTime.Now.Add(TimeSpan.FromSeconds(_votingDurationInSeconds));
                }

                ShouldRegenerate = true;
            }
        }

        private bool ShouldStartRapidfire() => IsRapidfire || _rapidfireChancePrecent == 0 ? false : _rng.Next(100) + 1 <= _rapidfireChancePrecent;

        internal double GetProgressPercentage()
        {
            var max = (_pollEndDateTime - _pollStartDateTime).TotalSeconds;
            var current = (DateTime.Now - _pollStartDateTime).TotalSeconds;

            return (current / max) * 100.0;
        }
    }
}
