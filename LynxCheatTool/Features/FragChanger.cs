using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager;
using CS2MenuManager.API.Menu;

namespace LynxCheatTool.Features;

[Flags]
public enum FragIcons
{
    None = 0,
    Headshot = 1 << 0,
    Blind = 1 << 1,
    Smoke = 1 << 2,
    Wallbang = 1 << 3,
    Noscope = 1 << 4,
    Airborne = 1 << 5,
    Dominated = 1 << 6,
    Revenge = 1 << 7,
    Wipe = 1 << 8,
    All = Headshot | Blind | Smoke | Wallbang | Noscope | Airborne | Dominated | Revenge | Wipe
}

public class FragChanger
{
    private readonly LynxCheatTool _plugin;
    private readonly Dictionary<ulong, FragIcons> _fragChangerSettings = new();

    public FragChanger(LynxCheatTool plugin)
    {
        _plugin = plugin;
    }

    public void OnFragChangerCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.FragChangerPermission))
        {
            player.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Red}You do not have permission to use this command!{ChatColors.Default}");
            return;
        }

        ShowFragChangerWasdMenu(player);
    }

    private void ShowFragChangerWasdMenu(CCSPlayerController admin)
    {
        var allPlayers = Utilities.GetPlayers()
            .Where(p => p != null && p.IsValid)
            .OrderBy(p => p.PlayerName)
            .ToList();

        if (allPlayers.Count == 0)
        {
            admin.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Red}No players found on the server!{ChatColors.Default}");
            return;
        }

        WasdMenu menu = new(_plugin.Config.MenuTitle + " Frag Changer Menu", _plugin);

        menu.AddItem("👥 Toggle All", (p, o) => 
        {
            ToggleFragChangerAll(p);
            ShowFragChangerWasdMenu(p);
        });

        foreach (var targetPlayer in allPlayers)
        {
            var steamId = targetPlayer.SteamID;
            var currentSettings = _fragChangerSettings.TryGetValue(steamId, out var settings) ? settings : FragIcons.None;
            var isEnabled = currentSettings != FragIcons.None;
            
            var statusIcon = isEnabled ? "✓" : "✗";
            var teamName = targetPlayer.TeamNum == 2 ? "[T]" : targetPlayer.TeamNum == 3 ? "[CT]" : "[SPEC]";
            var displayName = $"{statusIcon} {teamName} {targetPlayer.PlayerName} (Settings)";

            menu.AddItem(displayName, (p, o) =>
            {
                ShowFragChangerDetailMenu(p, targetPlayer);
            });
        }

        menu.Display(admin, 30);
    }

    private void ShowFragChangerDetailMenu(CCSPlayerController admin, CCSPlayerController targetPlayer)
    {
        WasdMenu menu = new($"Frag Settings: {targetPlayer.PlayerName}", _plugin);
        var steamId = targetPlayer.SteamID;
        
        if (!_fragChangerSettings.ContainsKey(steamId))
            _fragChangerSettings[steamId] = FragIcons.None;

        var currentSettings = _fragChangerSettings[steamId];

        menu.AddItem("🔄 Toggle All", (p, o) =>
        {
            if (_fragChangerSettings[steamId] == FragIcons.All)
                _fragChangerSettings[steamId] = FragIcons.None;
            else
                _fragChangerSettings[steamId] = FragIcons.All;

            ShowFragChangerDetailMenu(p, targetPlayer);
        });

        void AddToggleItem(string name, FragIcons icon)
        {
            bool isActive = currentSettings.HasFlag(icon);
            string status = isActive ? "✓" : "✗";
            menu.AddItem($"{status} {name}", (p, o) =>
            {
                if (isActive)
                    _fragChangerSettings[steamId] &= ~icon;
                else
                    _fragChangerSettings[steamId] |= icon;
                
                ShowFragChangerDetailMenu(p, targetPlayer);
            });
        }

        AddToggleItem("Headshot", FragIcons.Headshot);
        AddToggleItem("Blind", FragIcons.Blind);
        AddToggleItem("Smoke", FragIcons.Smoke);
        AddToggleItem("Wallbang", FragIcons.Wallbang);
        AddToggleItem("Noscope", FragIcons.Noscope);
        AddToggleItem("AttackerAir", FragIcons.Airborne);
        AddToggleItem("Domination", FragIcons.Dominated);
        AddToggleItem("Revenge", FragIcons.Revenge);
        AddToggleItem("Wipe", FragIcons.Wipe);

        menu.AddItem("⬅️ Back", (p, o) => ShowFragChangerWasdMenu(p));

        menu.Display(admin, 30);
    }

    private void ToggleFragChangerAll(CCSPlayerController admin)
    {
        var allPlayers = Utilities.GetPlayers().Where(p => p != null && p.IsValid).ToList();
        bool anyEnabled = allPlayers.Any(p => _fragChangerSettings.TryGetValue(p.SteamID, out var s) && s != FragIcons.None);
        
        var newSettings = anyEnabled ? FragIcons.None : FragIcons.All;

        foreach (var player in allPlayers)
        {
            _fragChangerSettings[player.SteamID] = newSettings;
        }

        string stateText = newSettings == FragIcons.All ? "All Icons Enabled" : "All Icons Disabled";
        admin.PrintToCenter($"All players Icons changed status to {stateText}");
    }

    public HookResult OnPlayerDeath(EventPlayerDeath @event)
    {
        if (@event?.Attacker != null && @event.Attacker.IsValid)
        {
            var attackerSteamId = @event.Attacker.SteamID;

            if (_fragChangerSettings.TryGetValue(attackerSteamId, out var settings) && settings != FragIcons.None)
            {
                if (settings.HasFlag(FragIcons.Headshot)) @event.Headshot = true;
                if (settings.HasFlag(FragIcons.Blind)) @event.Attackerblind = true;
                if (settings.HasFlag(FragIcons.Smoke)) @event.Thrusmoke = true;
                if (settings.HasFlag(FragIcons.Wallbang)) @event.Penetrated = 1;
                if (settings.HasFlag(FragIcons.Noscope)) @event.Noscope = true;
                if (settings.HasFlag(FragIcons.Dominated)) @event.Dominated = 1;
                if (settings.HasFlag(FragIcons.Revenge)) @event.Revenge = 1;
                if (settings.HasFlag(FragIcons.Wipe)) @event.Wipe = 1;
                if (settings.HasFlag(FragIcons.Airborne)) @event.Attackerinair = true;
                
                return HookResult.Changed;
            }
        }
        return HookResult.Continue;
    }
}
