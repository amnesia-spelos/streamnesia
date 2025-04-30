using System.Net.Sockets;
using System.Net;
using Streamnesia.Client.Extensions;
using Streamnesia.Client.Hubs;
using QRCoder;
using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddStreamnesiaDependencies(builder.Configuration);
builder.Services.AddSingleton<AmnesiaClientEventDispatcher>();
builder.Services.AddSingleton<TwitchPollDispatcher>();
builder.Services.AddSingleton<TwitchClientEventDispatcher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapHub<StatusHub>("/statushub");
app.MapHub<OverlayHub>("/overlayhub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var localIp = GetLocalIPAddress();
    var url = $"http://{localIp}:5000";

    var qrGenerator = new QRCodeGenerator();
    var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
    var asciiQr = new AsciiQRCode(qrCodeData);
    string qrCodeAsAscii = asciiQr.GetGraphic(1);

    Console.WriteLine("\nScan this QR code to open the app:");
    Console.WriteLine(qrCodeAsAscii);
    Console.WriteLine($"Or go to {url} on your phone/laptop on the same network");
});

// ensuring this gets activated on startup
app.Services.GetRequiredService<AmnesiaClientEventDispatcher>();
app.Services.GetRequiredService<TwitchPollDispatcher>();
app.Services.GetRequiredService<TwitchClientEventDispatcher>();

await app.RunAsync();

static string GetLocalIPAddress()
{
    foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
    {
        if (ni.OperationalStatus != OperationalStatus.Up)
            continue;

        // Skip virtual or tunnel interfaces
        if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
            ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
            continue;

        var ipProps = ni.GetIPProperties();

        foreach (var addr in ipProps.UnicastAddresses)
        {
            if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                !IPAddress.IsLoopback(addr.Address))
            {
                // Filter out VM-related or virtual adapters if needed
                var ip = addr.Address.ToString();

                // prioritize 192.168.x.x over 10.x or 172.x if multiple exist
                if (ip.StartsWith("192.168.100."))
                {
                    return ip;
                }
            }
        }
    }

    return "localhost";
}
