using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace LynxCheatTool;

public class LynxCheatToolConfig : BasePluginConfig
{
    [JsonPropertyName("ChatTag")]
    public string ChatTag { get; set; } = "[LynxCheatTool]";

    [JsonPropertyName("MenuTitle")]
    public string MenuTitle { get; set; } = "LynxCheatTool";

    [JsonPropertyName("MenuPermission")]
    public string MenuPermission { get; set; } = "@css/root";

    [JsonPropertyName("AimbotPermission")]
    public string AimbotPermission { get; set; } = "@css/root";

    [JsonPropertyName("WallhackPermission")]
    public string WallhackPermission { get; set; } = "@css/root";

    [JsonPropertyName("MagicBulletPermission")]
    public string MagicBulletPermission { get; set; } = "@css/root";

    [JsonPropertyName("NoRecoilPermission")]
    public string NoRecoilPermission { get; set; } = "@css/root";

    [JsonPropertyName("NoFlashPermission")]
    public string NoFlashPermission { get; set; } = "@css/root";

    [JsonPropertyName("FragChangerPermission")]
    public string FragChangerPermission { get; set; } = "@css/root";

    [JsonPropertyName("BunnyHopPermission")]
    public string BunnyHopPermission { get; set; } = "@css/root";

    // Aimbot Settings
    [JsonPropertyName("AimbotFOV")]
    public float AimbotFOV { get; set; } = 200f;

    [JsonPropertyName("AimbotStickyFOV")]
    public float AimbotStickyFOV { get; set; } = 360f;

    [JsonPropertyName("AimbotSmooth")]
    public float AimbotSmooth { get; set; } = 1f;

    // Wallhack Settings
    [JsonPropertyName("WallhackColorR")]
    public int WallhackColorR { get; set; } = 255;

    [JsonPropertyName("WallhackColorG")]
    public int WallhackColorG { get; set; } = 0;

    [JsonPropertyName("WallhackColorB")]
    public int WallhackColorB { get; set; } = 0;

    // Command Names
    [JsonPropertyName("MainMenuCommand")]
    public string MainMenuCommand { get; set; } = "css_cheats";

    [JsonPropertyName("AimbotCommand")]
    public string AimbotCommand { get; set; } = "css_aimbot";

    [JsonPropertyName("WallhackCommand")]
    public string WallhackCommand { get; set; } = "css_wallhack";

    [JsonPropertyName("MagicBulletCommand")]
    public string MagicBulletCommand { get; set; } = "css_magicbullet";

    [JsonPropertyName("NoRecoilCommand")]
    public string NoRecoilCommand { get; set; } = "css_norecoil";

    [JsonPropertyName("NoFlashCommand")]
    public string NoFlashCommand { get; set; } = "css_noflash";

    [JsonPropertyName("FragChangerCommand")]
    public string FragChangerCommand { get; set; } = "css_fragchanger";

    [JsonPropertyName("BunnyHopCommand")]
    public string BunnyHopCommand { get; set; } = "css_bunnyhop";
}
