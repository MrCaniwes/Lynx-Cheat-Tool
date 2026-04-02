using System.Text.RegularExpressions;

namespace LynxPlugin.Extensions;

public static class ChatExtensions
{
    private static readonly Dictionary<string, string> ColorMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "{DEFAULT}", "\u0001" },
        { "{WHITE}", "\u0001" },
        { "{DARKRED}", "\u0002" },
        { "{PURPLE}", "\u0003" },
        { "{GREEN}", "\u0004" },
        { "{PALEGREEN}", "\u0005" },
        { "{LIME}", "\u0006" },
        { "{RED}", "\u0007" },
        { "{GREY}", "\u0008" },
        { "{YELLOW}", "\u0009" },
        { "{GOLD}", "\u0010" },
        { "{SILVER}", "\u000A" },
        { "{BLUE}", "\u000B" },
        { "{DARKBLUE}", "\u000C" },
        { "{BLUEGRAY}", "\u000D" },
        { "{MAGENTA}", "\u000E" },
        { "{LIGHTRED}", "\u000F" },
        { "{ORANGE}", "\u0010" }
    };

    public static string ReplaceColorTags(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        input = "\u0001" + input;

        foreach (var color in ColorMapping)
        {
            input = Regex.Replace(input, Regex.Escape(color.Key), color.Value, RegexOptions.IgnoreCase);
        }

        return input;
    }
}
