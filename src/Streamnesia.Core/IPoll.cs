using FluentResults;

namespace Streamnesia.Core;

public interface IPoll<TItem> where TItem : class
{
    IReadOnlyCollection<TItem>? Options { get; }

    void SetOptions(IEnumerable<TItem> options);

    Result SetNamedVote(string name, int optionIndex);

    int GetVotesByOptionIndex(int optionIndex);

    void ClearAllVotes();

    TItem GetVotedItem();
}
