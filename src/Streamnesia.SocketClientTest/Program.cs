using System.Net.Sockets;
using System.Text;

const string host = "127.0.0.1";
const int port = 5150;

Console.WriteLine("Attempting to connect in 5 seconds!");
await Task.Delay(TimeSpan.FromSeconds(5));
Console.Clear();
Console.WriteLine("Trying to connect now...");

try
{
    using var client = new TcpClient();
    client.Connect(host, port);
    Console.WriteLine($"Connected to {host}:{port}");

    using NetworkStream stream = client.GetStream();
    var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
    var reader = new StreamReader(stream, Encoding.ASCII);

    // Read welcome message
    string welcome = await reader.ReadLineAsync();
    Console.WriteLine($"Game says: {welcome}");

    while (true)
    {
        Console.Write(">> ");
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            break;

        writer.WriteLine(input);

        string response = await reader.ReadLineAsync();
        Console.WriteLine($"Game says: {response}");
    }
}
catch (SocketException ex)
{
    Console.WriteLine($"Connection failed: {ex.Message}");
}

Console.WriteLine("Disconnected. Press any key to exit...");
Console.ReadKey();
