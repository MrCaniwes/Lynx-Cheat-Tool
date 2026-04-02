using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using LynxPlugin.Extensions;
using LynxPlugin.Features;
using LynxPlugin;

namespace LynxCheat;

public class LynxPlugin : BasePlugin, IPluginConfig<LynxConfig>
{
    public override string ModuleName => "Lynx Cheat Tool";
    public override string ModuleVersion => "1.1.2";
    public override string ModuleAuthor => "Antigravity & Çağan";

    public LynxConfig Config { get; set; } = new();
    private readonly List<IFeature> _features = new();
    private EspFeature? _esp;
    private OneTapFeature? _onetap;
    private NoFlashFeature? _noflash;
    private BhopFeature? _bhop;
    private AimbotFeature? _aimbot;
    private InvisibilityFeature? _invis;

    public override void Load(bool hotReload)
    {
        _esp = new EspFeature();
        _onetap = new OneTapFeature();
        _noflash = new NoFlashFeature();
        _bhop = new BhopFeature();
        _aimbot = new AimbotFeature();
        _invis = new InvisibilityFeature();
        _features.Add(_esp);
        _features.Add(_onetap);
        _features.Add(_noflash);
        _features.Add(_bhop);
        _features.Add(_aimbot);
        _features.Add(_invis);

        foreach (var feature in _features)
        {
            feature.OnLoad(this);
            Console.WriteLine($"[Lynx] {feature.Name} aktifleşti.");
        }

        AddCommand($"css_{Config.StatusCommand}", "Check cheat status for a player", (player, info) =>
        {
            if (player == null) return;

            string targetArg = info.GetArg(1);
            CCSPlayerController? target;

            if (string.IsNullOrEmpty(targetArg))
            {
                target = player;
            }
            else
            {
                target = Utilities.GetPlayers().FirstOrDefault(p => p != null && p.IsValid && p.PlayerName.Contains(targetArg, StringComparison.OrdinalIgnoreCase));
            }

            if (target == null)
            {
                player.PrintToChat($"{Config.Prefix.ReplaceColorTags()} {ChatColors.Red}Hata: {ChatColors.Default}Hedef '{targetArg}' bulunamadı!");
                return;
            }

            player.PrintToChat(Localizer["status.title", ChatColors.Blue, target.PlayerName].ToString().ReplaceColorTags());
            foreach (var feature in _features)
            {
                string statusKey = feature.IsEnabled(target) ? "status.enabled" : "status.disabled";
                string statusText = Localizer[statusKey].ToString().ReplaceColorTags();
                player.PrintToChat($"{ChatColors.LightPurple}{feature.Name}: {statusText}".ReplaceColorTags());
            }
            player.PrintToChat($"{ChatColors.Blue}---------------------------".ReplaceColorTags());
        });

        // Visibility filtresi (Hata vermemesi için son build yöntemimizle ekledim)
        RegisterListener<Listeners.CheckTransmit>(infoList => {
            if (_esp == null) return;
            foreach (var item in infoList) {
                if (item.Item2 == null || !item.Item2.IsValid) continue;
                if (!_esp.PlayersWithEsp.Contains((uint)item.Item2.Slot)) {
                    foreach (var entry in _esp.PlayerGlows.Values) {
                        if (entry.relay?.IsValid == true) item.Item1.TransmitEntities.Remove(entry.relay);
                        if (entry.glow?.IsValid == true) item.Item1.TransmitEntities.Remove(entry.glow);
                    }
                }
            }
        });
    }

    public void OnConfigParsed(LynxConfig config)
    {
        Config = config;
    }
}
