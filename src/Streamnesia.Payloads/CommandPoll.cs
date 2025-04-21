using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Streamnesia.Core;

namespace Streamnesia.Payloads;

public class CommandPoll //: ICommandPoll
{
    //private readonly Mutex _dataAccessMutex = new Mutex();
    //private readonly Random _random = new Random();

    //private IEnumerable<Payload> _availablePayloads;
    //private Payload[] _options;
    //private ICollection<string>[] _votes;

    //public void GenerateNew()
    //{
    //    _dataAccessMutex.WaitOne();

    //    if(_availablePayloads is null)
    //    {
    //        Console.WriteLine("Available payloads weren't set when a new poll was being generated.");
    //        _dataAccessMutex.ReleaseMutex();
    //        return;
    //    }

    //    if(_availablePayloads.Count() < 4)
    //    {
    //        Console.WriteLine($"There aren't enough payloads to generate a poll. There are {_availablePayloads.Count()} payloads.");
    //        _dataAccessMutex.ReleaseMutex();
    //        return;
    //    }

    //    _options = GetRandomUniquePayloadSelection();
    //    _votes = new[] { new List<string>(), new List<string>(), new List<string>(), new List<string>() };

    //    _dataAccessMutex.ReleaseMutex();
    //}

    //public Payload GetPayloadAt(int index)
    //{
    //    if (index < 0 || index > 3)
    //        throw new IndexOutOfRangeException();

    //    _dataAccessMutex.WaitOne();

    //    var result = _options[index];

    //    _dataAccessMutex.ReleaseMutex();

    //    if(string.IsNullOrWhiteSpace(result.Name))
    //        Console.WriteLine("GetPayloadAt received an empty-named Payload.");

    //    return result;
    //}

    //public Payload GetPayloadWithMostVotes()
    //{
    //    _dataAccessMutex.WaitOne();

    //    var topVotes = -1;
    //    var topCategories = new List<int>();

    //    for(var i = 0; i < 4; i++)
    //    {
    //        if(_votes[i].Count >= topVotes)
    //        {
    //            topVotes = _votes[i].Count;
    //            topCategories.Clear();
    //        }

    //        if(_votes[i].Count == topVotes)
    //        {
    //            topCategories.Add(i);
    //        }
    //    }

    //    var result = _options[topCategories.Random(_random)];

    //    _dataAccessMutex.ReleaseMutex();

    //    return result;
    //}

    //public IEnumerable<PollOption> GetPollOptions()
    //{
    //    _dataAccessMutex.WaitOne();

    //    var result = new PollOption[_options.Length];
    //    for(var i = 0; i < _options.Length; i++)
    //    {
    //        result[i] = ToPollOption(_options[i], i);
    //    }

    //    _dataAccessMutex.ReleaseMutex();

    //    return result;
    //}

    //public void SetPayloadSource(IEnumerable<Payload> payloads)
    //{
    //    _dataAccessMutex.WaitOne();

    //    _availablePayloads = payloads;

    //    _dataAccessMutex.ReleaseMutex();
    //}

    //public void Vote(string displayname, int vote)
    //{
    //    _dataAccessMutex.WaitOne();

    //    foreach(var group in _votes)
    //    {
    //        group.Remove(displayname);
    //    }

    //    _votes[vote].Add(displayname);

    //    _dataAccessMutex.ReleaseMutex();
    //}

    //private Payload[] GetRandomUniquePayloadSelection()
    //{
    //    var result = new Payload[4];

    //    Payload candidate;
    //    for (var i = 0; i < result.Length; i++)
    //    {
    //        do candidate = _availablePayloads.Random(_random);
    //        while (result.Any(o => o.Name == candidate.Name));

    //        result[i] = candidate;
    //    }

    //    return result;
    //}

    //private PollOption ToPollOption(Payload payload, int index)
    //{
    //    return new PollOption
    //    {
    //        Index = index,
    //        Name = payload.Name,
    //        Votes = _votes[index].Count
    //    };
    //}
}
