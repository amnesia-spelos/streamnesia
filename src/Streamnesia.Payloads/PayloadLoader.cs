using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using Streamnesia.Core;
using Streamnesia.Core.Configuration;
using Streamnesia.Core.Entities;

namespace Streamnesia.Payloads;

public class PayloadLoader(
    HttpClient httpClient,
    IConfigurationStorage configStorage,
    ILogger<PayloadLoader> logger) : IPayloadLoader
{
    private const string PayloadsDirectory = "payloads";
    private const string PayloadsFile = "payloads.json";
    private const string PayloadsUrl = "https://github.com/amnesia-spelos/streamnesia-payloads/archive/main.zip";
    private const string PayloadZipFile = "main.zip";
    private const string PayloadExtractionDirectory = "main-payloads";
    private const string UnpackedPayloadMainDirectory = "streamnesia-payloads-main";

    public IReadOnlyCollection<ParsedPayload>? Payloads { get; private set; }

    private PayloadLoaderConfig? _currentConfig;

    public async Task<Result> LoadPayloadsAsync(CancellationToken cancellationToken = default)
    {
        _currentConfig = configStorage.ReadPayloadLoaderConfig();

        if (_currentConfig is null)
        {
            logger.LogError("Failed to read config");
            return Result.Fail("Failed to read config");
        }

        if (!LocalPayloadsExist() && !_currentConfig.DownloadEnabled)
        {
            logger.LogError("Cannot continue because payload download is disabled and no local payloads exist.");
            return Result.Fail("No payloads exist and download is disabled");
        }

        if (_currentConfig.DownloadEnabled)
        {
            var downloadResult = await DownloadAndExtractPayloads(cancellationToken);

            if (downloadResult.IsFailed)
                return Result.Fail(downloadResult.Errors);
        }

        var loadResult = LoadLocalPayloads();

        return loadResult;
    }

    private static bool LocalPayloadsExist() => File.Exists(Path.Combine(PayloadsDirectory, PayloadsFile));

    private async Task<Result> DownloadAndExtractPayloads(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync(PayloadsUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var fileStream = new FileStream(PayloadZipFile, FileMode.Create, FileAccess.Write, FileShare.None);
            await using var httpStream = await response.Content.ReadAsStreamAsync(cancellationToken);

            await httpStream.CopyToAsync(fileStream, cancellationToken);

            fileStream.Close();
            httpStream.Close();

            logger.LogInformation("Downloaded payloads .zip file");

            if (Directory.Exists(PayloadExtractionDirectory))
            {
                logger.LogInformation("Deleting old extraction directory");
                Directory.Delete(PayloadExtractionDirectory, true);
            }

            ZipFile.ExtractToDirectory(PayloadZipFile, PayloadExtractionDirectory);
            File.Delete(Path.Combine(PayloadExtractionDirectory, UnpackedPayloadMainDirectory, "LICENSE"));
            File.Delete(Path.Combine(PayloadExtractionDirectory, UnpackedPayloadMainDirectory, "README.md"));
            logger.LogInformation("Extracted payloads .zip file");

            // NOTE(spelos): A better setup for debugging purposes would be nice
            await CopyDirectoryAsync(Path.Combine(PayloadExtractionDirectory, UnpackedPayloadMainDirectory), "..", copySubDirs: true);
            logger.LogInformation("Copied extracted files");

            File.Delete(PayloadZipFile);
            Directory.Delete(PayloadExtractionDirectory, recursive: true);
            logger.LogInformation("Cleaned up downloaded resources");

            return Result.Ok();
        }
        catch (HttpRequestException e)
        {
            logger.LogError(e, "Failed to download payloads");
            return Result.Fail($"Failed to download payloads: {e.Message}");
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred during download or unpacking of payloads");
            return Result.Fail($"Failed to download or extract payloads: {e.Message}");
        }
    }

    // NOTE(spelos): a candidate for inclusion in a possible IFileSystem abstraction
    public static async Task CopyDirectoryAsync(string sourceDir, string destDir, bool copySubDirs = true)
    {
        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.EnumerateFiles(sourceDir))
        {
            var destFile = Path.Join(destDir, Path.GetFileName(file));
            await using var sourceStream = File.OpenRead(file);
            await using var destStream = File.Create(destFile);
            await sourceStream.CopyToAsync(destStream);
        }

        if (copySubDirs)
        {
            foreach (var subDir in Directory.EnumerateDirectories(sourceDir))
            {
                var destSubDir = Path.Join(destDir, Path.GetFileName(subDir));
                await CopyDirectoryAsync(subDir, destSubDir, copySubDirs);
            }
        }
    }

    private Result LoadLocalPayloads()
    {
        if (_currentConfig is null)
            return Result.Fail("_currentConfig was null");

        ICollection<PayloadModel> payloads = [];

        if (_currentConfig.UseVanillaPayloads)
        {
            if (!Directory.Exists(PayloadsDirectory))
            {
                logger.LogError("Payloads directory missing: {Directory}", PayloadsDirectory);
                return Result.Fail("Payloads directory missing");
            }

            try
            {
                var json = File.ReadAllText(Path.Combine(PayloadsDirectory, PayloadsFile));
                var vanillaPayloads = JsonSerializer.Deserialize<IEnumerable<PayloadModel>>(json);

                if (vanillaPayloads is null)
                {
                    logger.LogError("Failed to parse vanilla payloads JSON");
                    return Result.Fail("Failed to parse vanilla payloads JSON");
                }

                payloads = [.. payloads, .. vanillaPayloads];
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to read or parse vanilla payloads JSON");
                return Result.Fail("Failed to read or parse vanilla payloads JSON");
            }
        }

        if (!string.IsNullOrWhiteSpace(_currentConfig.CustomPayloadsFile))
        {
            if (File.Exists(_currentConfig.CustomPayloadsFile))
            {
                try
                {
                    var json = File.ReadAllText(_currentConfig.CustomPayloadsFile);
                    var customPayloads = JsonSerializer.Deserialize<IEnumerable<PayloadModel>>(json);

                    if (customPayloads is null)
                    {
                        logger.LogError("Failed to parse custom payloads JSON");
                        return Result.Fail("Failed to parse custom payloads JSON");
                    }

                    payloads = [.. payloads, .. customPayloads];
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to read or parse custom payloads JSON");
                    return Result.Fail("Failed to read or parse custom payloads JSON");
                }
            }
            else
            {
                logger.LogWarning("The custom payloads file '{File}' was not found.", _currentConfig.CustomPayloadsFile);
            }
        }

        if (payloads.Count == 0)
        {
            logger.LogError("No payloads loaded");
            return Result.Fail("No payloads loaded");
        }

        try
        {
            Payloads = [.. payloads.Select(p => new ParsedPayload
            {
                Name = p.Name,
                Sequence = ToParsedSequence(p.Sequence)
            })];

        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to parse payload sequences");
            return Result.Fail("Failed to parse payload sequences");
        }

        return Result.Ok();
    }

    private ParsedPayloadSequenceItem[] ToParsedSequence(PayloadSequenceModel[] sequence)
    {
        return [.. sequence.Select(i => new ParsedPayloadSequenceItem
        {
            AngelCode = GetPayloadFileText(i.File) ?? string.Empty,
            Delay = i.Delay
        })];
    }

    private string? GetPayloadFileText(string file)
    {
        if (file is null)
        {
            logger.LogWarning("Payload file was null");
            return null;
        }    

        return File.ReadAllText(Path.Combine(PayloadsDirectory, file));
    }
}
