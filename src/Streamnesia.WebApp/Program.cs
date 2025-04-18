using System;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Streamnesia.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        public static void Start(CancellationToken cancellationToken, IGuiServices guiServices)
        {
            _ = CreateHostBuilder(Array.Empty<string>(), guiServices).Build().RunAsync(cancellationToken);
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IGuiServices guiServices) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup(c => new Startup(c.Configuration, guiServices));
                });

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
