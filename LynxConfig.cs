using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace LynxPlugin;

public class LynxConfig : IBasePluginConfig
{
    [JsonPropertyName("Version")]
    public int Version { get; set; } = 1;

    [JsonPropertyName("Prefix")]
    public string Prefix { get; set; } = "{BLUE}[Lynx]{DEFAULT}";

    [JsonPropertyName("EspCommand")]
    public string EspCommand { get; set; } = "esp";

    [JsonPropertyName("OneTapCommand")]
    public string OneTapCommand { get; set; } = "onetap";

    [JsonPropertyName("NoFlashCommand")]
    public string NoFlashCommand { get; set; } = "noflash";

    [JsonPropertyName("BhopCommand")]
    public string BhopCommand { get; set; } = "bhop";

    [JsonPropertyName("AimbotCommand")]
    public string AimbotCommand { get; set; } = "aimbot";

    [JsonPropertyName("AimbotFOV")]
    public float AimbotFOV { get; set; } = 15.0f;

    [JsonPropertyName("AimbotSmooth")]
    public float AimbotSmooth { get; set; } = 0.1f;

    [JsonPropertyName("AimbotStickyFOV")]
    public float AimbotStickyFOV { get; set; } = 25.0f;

    [JsonPropertyName("AimbotFovCommand")]
    public string AimbotFovCommand { get; set; } = "aimfov";

    [JsonPropertyName("AimbotSmoothCommand")]
    public string AimbotSmoothCommand { get; set; } = "aimsmooth";

    [JsonPropertyName("AimbotStickyFovCommand")]
    public string AimbotStickyFovCommand { get; set; } = "aimsticky";

    [JsonPropertyName("InvisibilityCommand")]
    public string InvisibilityCommand { get; set; } = "inv";

    [JsonPropertyName("StatusCommand")]
    public string StatusCommand { get; set; } = "status";

    [JsonPropertyName("EspColorCt")]
    public string EspColorCt { get; set; } = "0 0 255 255";

    [JsonPropertyName("EspColorT")]
    public string EspColorT { get; set; } = "255 0 0 255";

    [JsonPropertyName("GlowType")]
    public int GlowType { get; set; } = 3;

    [JsonPropertyName("AllowedSteamIds")]
    public List<string> AllowedSteamIds { get; set; } = new() { "7656119XXXXXXXXXX" };

    [JsonPropertyName("AllowedFlags")]
    public List<string> AllowedFlags { get; set; } = new() { "@css/ban", "@css/cheats" };

    [JsonPropertyName("AllowedGroups")]
    public List<string> AllowedGroups { get; set; } = new() { "#admin" };
}
