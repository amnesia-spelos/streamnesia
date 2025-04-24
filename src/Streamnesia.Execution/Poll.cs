using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FluentResults;
using Microsoft.Extensions.Logging;
using Streamnesia.Core;

namespace Streamnesia.Execution;

public class Poll<TItem>(
    Random random,
    ILogger<Poll<TItem>> logger
) : IPoll<TItem> where TItem : class
{
    public IReadOnlyCollection<TItem>? Options { get; private set; }

    private readonly ConcurrentDictionary<string, int> _votesByIdDictionary = new();

    public void ClearAllVotes()
    {
        _votesByIdDictionary.Clear();
    }

    public void SetOptions(IEnumerable<TItem> options)
    {
        ClearAllVotes();
        Options = [.. options];
    }

    public Result SetNamedVote(string name, int optionIndex)
    {
        if (Options is null || Options.Count == 0)
        {
            logger.LogError("Null or no options");
            return Result.Fail("Null or no options");
        }

        if (optionIndex < 0 || optionIndex >= Options.Count)
        {
            logger.LogWarning("Vote index out of bounds");
            return Result.Fail("Vote index out of bounds");
        }

        if (_votesByIdDictionary.ContainsKey(name))
        {
            _votesByIdDictionary[name] = optionIndex;
            logger.LogInformation("Changed vote for id {Name} to {Index}", name, optionIndex);
        }
        else
        {
            var addSuccess = _votesByIdDictionary.TryAdd(name, optionIndex);
            if (!addSuccess)
            {
                logger.LogError("Failed to add new vote");
                return Result.Fail("Failed to add new vote");
            }

            logger.LogInformation("Vote {Name} for {Index} added", name, optionIndex);
        }

        return Result.Ok();
    }

    public TItem GetVotedItem()
    {
        if (Options is null || Options.Count == 0)
            throw new InvalidOperationException("Poll options not set or empty.");

        if (_votesByIdDictionary.IsEmpty)
        {
            logger.LogWarning("No votes received. Selecting random option.");
            return Options.ElementAt(random.Next(Options.Count));
        }

        var voteGroups = _votesByIdDictionary.Values
            .GroupBy(index => index)
            .Select(g => new { Index = g.Key, Count = g.Count() })
            .ToList();

        var maxVotes = voteGroups.Max(g => g.Count);
        var topOptions = voteGroups
            .Where(g => g.Count == maxVotes)
            .Select(g => g.Index)
            .ToList();

        var selectedIndex = topOptions[random.Next(topOptions.Count)];
        return Options.ElementAt(selectedIndex);
    }

    public int GetVotesByOptionIndex(int optionIndex)
    {
        logger.LogInformation("Requesting votes for option with index {Index}", optionIndex);

        if (Options is null || Options.Count == 0 || optionIndex < 0 || optionIndex >= Options.Count)
        {
            logger.LogWarning("Requested votes for invalid option");
            return 0;
        }

        return _votesByIdDictionary.Values.Count(v => v == optionIndex);
    }
}
