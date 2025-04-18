using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Streamnesia.CommandProcessing;

public partial class AmnesiaClient(Random random) : IDisposable
{
    const string Host = "127.0.0.1";
    const int Port = 5150;

    private static readonly string[] MusicFiles = [
        "03_event_books.ogg",
        "29_event_end.ogg",
        "26_event_brute.ogg",
        "04_event_stairs.ogg",
        "24_event_vision02.ogg",
        "24_event_vision04.ogg",
        "00_event_gallery.ogg",
        "24_event_vision03.ogg",
        "21_event_pit.ogg",
        "27_event_bang.ogg",
        "19_event_brute.ogg",
        "15_event_prisoner.ogg",
        "05_event_falling.ogg",
        "26_event_agrippa_head.ogg",
        "05_event_steps.ogg",
        "00_event_hallway.ogg",
        "11_event_tree.ogg",
        "01_event_critters.ogg",
        "04_event_hole.ogg",
        "24_event_vision.ogg",
        "20_event_darkness.ogg",
        "15_event_elevator.ogg",
        "22_event_trapped.ogg",
        "15_event_girl_mother.ogg",
        "12_event_blood.ogg",
        "10_event_coming.ogg",
        "01_event_dust.ogg",
        "11_event_dog.ogg",
        "03_event_tomb.ogg"
    ];
    
    private readonly Random _random = random;
    private readonly TcpClient _client = new();
    private NetworkStream _stream;
    private StreamWriter _writer;
    private StreamReader _reader;
    private readonly Stopwatch _stopwatch = new();

    public async Task AttachToGameAsync()
    {
        _client.Connect(Host, Port);
        _stream = _client.GetStream();
        _writer = new StreamWriter(_stream, Encoding.ASCII) { AutoFlush = true };
        _reader = new StreamReader(_stream, Encoding.ASCII);

        Console.WriteLine("Waiting for the game to respond...");
        string welcome = await _reader.ReadLineAsync(); // TODO: Cancellation token + retry policy?

        if (!welcome.StartsWith("Hello, from Amnesia: The Dark Descent!"))
        {
            throw new InvalidOperationException($"Unexpected welcome message: '{welcome}'");
        }
        else
        {
            Console.WriteLine("Game connection established");
        }
    }

    public async Task DisplayTextAsync(string text)
    {
        if(string.IsNullOrWhiteSpace(text))
            return;
        
        Sanitize(ref text);

        await SendToAmnesiaAsync($"SetMessageExact(\"{text}\", 0.0f)");
    }

    public async Task SetDeathHintTextAsync(string text)
    {
        if(string.IsNullOrWhiteSpace(text))
            return;

        Sanitize(ref text);

        await SendToAmnesiaAsync($"SetDeathHint(\"StreamnesiaLiteral\", \"{text}\")");
    }

    public Task ExecuteAsync(string angelcode)
    {
        Minify(ref angelcode);
        Format(ref angelcode);

        return SendToAmnesiaAsync(angelcode);
    }

    private static void Minify(ref string angelcode)
    {
        angelcode = MinifyRegex().Replace(angelcode, "").Trim();
    }

    private void Format(ref string angelcode)
    {
        angelcode = string.Format(angelcode, GenerateGuids());
        angelcode = angelcode.Replace("<<RANDOM_MUSIC>>", GetRandomOggMusic());
    }

    private static string[] GenerateGuids()
        => Enumerable.Range(0, 10).Select(i => Guid.NewGuid().ToString().Replace("-", string.Empty)).ToArray();

    private async Task SendToAmnesiaAsync(string angelcode)
    {
        Console.WriteLine($"sending: exec:{angelcode}");
        _stopwatch.Restart();
        _writer.Write($"exec:{angelcode}");
        var response = await _reader.ReadLineAsync();
        _stopwatch.Stop();
        Console.WriteLine($"Game Response: {response} - (took {_stopwatch.ElapsedMilliseconds}ms)");
    }

    private static void Sanitize(ref string text)
    {
        text = MinifyRegex().Replace(text, "").Trim();
        text = text.Replace(";", string.Empty).Replace("\\", string.Empty).Replace("\"", "\\\"");
    }

    private string GetRandomOggMusic() => MusicFiles[_random.Next(0, MusicFiles.Length)];

    public void Dispose()
    {
        _client.Dispose();
        _stream.Dispose();

        GC.SuppressFinalize(this);
    }

    [GeneratedRegex(@"\t|\n|\r")]
    private static partial Regex MinifyRegex();
}
