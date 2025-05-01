using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Streamnesia.Configuration;

public class JsonFileStoredItem<TItem> : IStoredItem<TItem> where TItem : class
{
    private readonly ILogger<JsonFileStoredItem<TItem>> _logger;
    private readonly FileInfo _jsonFileInfo;

    public JsonFileStoredItem(IFileSystemPaths paths, ILogger<JsonFileStoredItem<TItem>> logger)
    {
        _logger = logger;

        var fileName = $"{typeof(TItem).Name}.json";
        _jsonFileInfo = new(Path.Combine(paths.ApplicationSettingsDirectory.FullName, fileName));
    }

    public bool Exists => File.Exists(_jsonFileInfo.FullName);

    public Result Overwrite(TItem item)
    {
        try
        {
            var json = JsonSerializer.Serialize(item);
            File.WriteAllText(_jsonFileInfo.FullName, json);
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to write JSON file");
            return Result.Fail("Failed to write JSON file");
        }
    }

    public Result<TItem?> Retrieve()
    {
        if (!Exists)
            return Result.Ok<TItem?>(null);

        try
        {
            var json = File.ReadAllText(_jsonFileInfo.FullName);
            var result = JsonSerializer.Deserialize<TItem?>(json);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to read or parse JSON file");
            return Result.Fail("Failed to read or parse JSON file");
        }
    }
}
