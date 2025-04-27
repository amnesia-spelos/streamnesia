using System;
using System.Linq;
using System.Text.RegularExpressions;
using Streamnesia.Core;

namespace Streamnesia.Payloads;

public partial class CommandPreprocessor(Random random) : ICommandPreprocessor
{

    [GeneratedRegex(@"\t|\n|\r")]
    private partial Regex MinifyRegex();

    public string PreprocessCommand(string command)
    {
        command = MinifyRegex().Replace(command, "").Trim();
        command = string.Format(command, GenerateGuids());
        command = command.Replace("<<RANDOM_MUSIC>>", GetRandomOggMusic());
        command = command.Replace("{{", "{");
        command = command.Replace("}}", "}");

        return command;
    }

    private static string[] GenerateGuids()
        => Enumerable.Range(0, 10).Select(i => Guid.NewGuid().ToString().Replace("-", string.Empty)).ToArray();

    private string GetRandomOggMusic() => MusicFiles[random.Next(0, MusicFiles.Length)];

    public string StringEscape(string str)
    {
        str = MinifyRegex().Replace(str, "").Trim();
        str = str.Replace(";", string.Empty).Replace("\\", string.Empty).Replace("\"", "\\\"");

        return str;
    }

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
}
