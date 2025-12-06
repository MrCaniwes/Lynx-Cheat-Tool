using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager;
using CS2MenuManager.API.Menu;

namespace LynxCheatTool.Features;

public class NoFlash
{
    private readonly LynxCheatTool _plugin;
    private readonly Dictionary<ulong, bool> _noFlashEnabled = new();

    public NoFlash(LynxCheatTool plugin)
    {
        _plugin = plugin;
    }

    public void OnNoFlashCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.NoFlashPermission))
        {
            player.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Red}You do not have permission to use this command!{ChatColors.Default}");
            return;
        }

        ShowNoFlashWasdMenu(player);
    }

    private void ShowNoFlashWasdMenu(CCSPlayerController admin)
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

        WasdMenu menu = new(_plugin.Config.MenuTitle + " No Flash Menu", _plugin);

        menu.AddItem("👥 Toggle All", (p, o) => 
        {
            ToggleNoFlashAll(p);
            ShowNoFlashWasdMenu(p);
        });

        foreach (var targetPlayer in allPlayers)
        {
            var steamId = targetPlayer.SteamID;
            var isEnabled = _noFlashEnabled.TryGetValue(steamId, out var enabled) && enabled;
            
            var statusIcon = isEnabled ? "✓" : "✗";
            var teamName = targetPlayer.TeamNum == 2 ? "[T]" : targetPlayer.TeamNum == 3 ? "[CT]" : "[SPEC]";
            var displayName = $"{statusIcon} {teamName} {targetPlayer.PlayerName}";

            menu.AddItem(displayName, (p, o) =>
            {
                ToggleNoFlash(p, targetPlayer);
                ShowNoFlashWasdMenu(p);
            });
        }

        menu.Display(admin, 30);
    }

    private void ToggleNoFlashAll(CCSPlayerController admin)
    {
        var allPlayers = Utilities.GetPlayers().Where(p => p != null && p.IsValid).ToList();
        bool anyEnabled = allPlayers.Any(p => _noFlashEnabled.TryGetValue(p.SteamID, out var e) && e);
        bool newState = !anyEnabled;

        foreach (var player in allPlayers)
        {
            _noFlashEnabled[player.SteamID] = newState;
        }

        string stateText = newState ? "Enabled" : "Disabled";
        admin.PrintToCenter($"All players No Flash {stateText}");
    }

    private void ToggleNoFlash(CCSPlayerController admin, CCSPlayerController targetPlayer)
    {
        var steamId = targetPlayer.SteamID;
        
        if (!_noFlashEnabled.ContainsKey(steamId))
            _noFlashEnabled[steamId] = false;

        _noFlashEnabled[steamId] = !_noFlashEnabled[steamId];

        if (_noFlashEnabled[steamId])
        {
            admin.PrintToCenter($"No Flash enabled for {targetPlayer.PlayerName}");
            targetPlayer.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.LightYellow}No Flash enabled! Sunglasses on! 😎{ChatColors.Default} (Admin: {admin.PlayerName})");
        }
        else
        {
            admin.PrintToCenter($"No Flash disabled for {targetPlayer.PlayerName}");
            targetPlayer.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Grey}No Flash disabled.{ChatColors.Default}");
        }
    }

    public void OnTick()
    {
        var players = Utilities.GetPlayers();
        
        foreach (var player in players)
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive)
                continue;

            if (_noFlashEnabled.TryGetValue(player.SteamID, out var noFlashEnabled) && noFlashEnabled)
            {
                var playerPawn = player.PlayerPawn.Value;
                if (playerPawn != null)
                {
                    playerPawn.FlashDuration = 0;
                }
            }
        }
    }
}
