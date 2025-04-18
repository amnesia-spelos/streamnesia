using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Streamnesia.Payloads.Entities;

namespace Streamnesia.Payloads
{
    public class LocalPayloadLoader : IPayloadLoader
    {
        private const string PayloadsDirectory = "payloads";
        
        private readonly StreamnesiaConfig _config;

        public LocalPayloadLoader(StreamnesiaConfig config)
        {
            _config = config;
        }

        public Task<IEnumerable<Payload>> GetPayloadsAsync()
        {
            var payloads = GetLocalPayloads();

            return Task.FromResult(payloads);
        }

        private IEnumerable<Payload> GetLocalPayloads()
        {
            if(_config.DownloadLatestPayloads && HasInternetConnection())
            {
                System.Console.WriteLine("Downloading payloads...");
                UpdatePayloads();
            }

            IEnumerable<PayloadModel> payloadDefinitions = new List<PayloadModel>();

            if(_config.UseVanillaPayloads)
            {
                if(!Directory.Exists(PayloadsDirectory))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("(!) The default payload directory does not exist (!)");
                    Console.WriteLine("Make sure to enable payload downloads for Streamnesia to auto-fix this issue.");
                    Console.ResetColor();
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                var json = File.ReadAllText(Path.Combine(PayloadsDirectory, "payloads.json"));
                payloadDefinitions = JsonSerializer.Deserialize<IEnumerable<PayloadModel>>(json);
            }

            if(File.Exists(_config.CustomPayloadsFile))
            {
                var customPayloads = JsonSerializer.Deserialize<IEnumerable<PayloadModel>>(File.ReadAllText(_config.CustomPayloadsFile));
                payloadDefinitions = payloadDefinitions.Concat(customPayloads);
            }

            if(payloadDefinitions.Count() == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("(!) There are no payloads (!)");
                Console.WriteLine("You can enable vanilla payloads and payload downloads in the config.");
                Console.WriteLine("Or you can define your own payloads.\n\n");
                Console.ResetColor();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            return payloadDefinitions.Select(p => new Payload
            {
                Name = p.Name,
                Sequence = ToCoreEntity(p.Sequence)
            });
        }

        private SequenceItem[] ToCoreEntity(SequenceModel[] sequence)
        {
            return sequence.Select(i => new SequenceItem
            {
                AngelCode = GetPayloadFileText(i.File),
                Delay = i.Delay
            }).ToArray();
        }

        private static void UpdatePayloads()
        {
            const string PayloadsUrl = "https://github.com/amnesia-spelos/streamnesia-payloads/archive/main.zip";

            using (var client = new WebClient())
            {
                if(System.IO.Directory.Exists("main-payloads"))
                {
                    System.IO.Directory.Delete("main-payloads", true);
                }

                client.DownloadFile(PayloadsUrl,  @"main.zip");
                System.IO.Compression.ZipFile.ExtractToDirectory(@"main.zip", "main-payloads");
                System.IO.File.Delete(@"main-payloads\streamnesia-payloads-main\LICENSE");
                System.IO.File.Delete(@"main-payloads\streamnesia-payloads-main\README.md");
                DirectoryCopy(@"main-payloads\streamnesia-payloads-main", "..", true);
                System.IO.File.Delete(@"main.zip");
                System.IO.Directory.Delete("main-payloads", true);
            }

            System.Console.WriteLine("Finished downloading payloads!");
        }

    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, true);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }
    }

        public static bool HasInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                    using (client.OpenRead("http://google.com/generate_204")) 
                        return true; 
            }
            catch
            {
                return false;
            }
        }

        private string GetPayloadFileText(string file)
        {
            if(file is null)
                return null;

            return File.ReadAllText(Path.Combine(PayloadsDirectory, file));
        }

        private string FormatPayloadName(string name)
            => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.Replace("-", " ").ToLower());
    }
}
