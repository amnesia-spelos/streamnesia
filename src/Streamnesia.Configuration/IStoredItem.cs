using FluentResults;

namespace Streamnesia.Configuration;

public interface IStoredItem<TItem> where TItem : class
{
    bool Exists { get; }

    Result<TItem?> Retrieve();

    Result Overwrite(TItem item);
}
