using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager;
using CS2MenuManager.API.Menu;

namespace LynxCheatTool.Features;

public class BunnyHop
{
    private readonly LynxCheatTool _plugin;
    private readonly Dictionary<ulong, bool> _bunnyHopEnabled = new();

    public BunnyHop(LynxCheatTool plugin)
    {
        _plugin = plugin;
    }

    public void OnBunnyHopCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.BunnyHopPermission))
        {
            player.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Red}You do not have permission to use this command!{ChatColors.Default}");
            return;
        }

        ShowBunnyHopWasdMenu(player);
    }

    private void ShowBunnyHopWasdMenu(CCSPlayerController admin)
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

        WasdMenu menu = new(_plugin.Config.MenuTitle + " " + "BunnyHop Menu", _plugin);

        menu.AddItem("Toggle All", (p, o) => 
        {
            ToggleBunnyHopAll(p);
            ShowBunnyHopWasdMenu(p);
        });

        foreach (var targetPlayer in allPlayers)
        {
            var steamId = targetPlayer.SteamID;
            var isEnabled = _bunnyHopEnabled.TryGetValue(steamId, out var enabled) && enabled;
            
            var statusIcon = isEnabled ? "✓" : "✗";
            var teamName = targetPlayer.TeamNum == 2 ? "[T]" : targetPlayer.TeamNum == 3 ? "[CT]" : "[SPEC]";
            var displayName = $"{statusIcon} {teamName} {targetPlayer.PlayerName}";

            menu.AddItem(displayName, (p, o) =>
            {
                ToggleBunnyHop(p, targetPlayer);
                ShowBunnyHopWasdMenu(p);
            });
        }

        menu.Display(admin, 30);
    }

    private void ToggleBunnyHopAll(CCSPlayerController admin)
    {
        var allPlayers = Utilities.GetPlayers().Where(p => p != null && p.IsValid).ToList();
        bool anyEnabled = allPlayers.Any(p => _bunnyHopEnabled.TryGetValue(p.SteamID, out var e) && e);
        bool newState = !anyEnabled;

        foreach (var player in allPlayers)
        {
            _bunnyHopEnabled[player.SteamID] = newState;
        }

        string stateText = newState ? "Enabled" : "Disabled";
        admin.PrintToCenter($"All players Bunny Hop {stateText}");
    }

    private void ToggleBunnyHop(CCSPlayerController admin, CCSPlayerController targetPlayer)
    {
        var steamId = targetPlayer.SteamID;
        
        if (!_bunnyHopEnabled.ContainsKey(steamId))
            _bunnyHopEnabled[steamId] = false;

        _bunnyHopEnabled[steamId] = !_bunnyHopEnabled[steamId];

        if (_bunnyHopEnabled[steamId])
        {
            admin.PrintToCenter($"Bunny Hop enabled for {targetPlayer.PlayerName}");
            targetPlayer.PrintToChat($"Bunny Hop enabled for {targetPlayer.PlayerName} (Admin: {admin.PlayerName})");
        }
        else
        {
            admin.PrintToCenter($"Bunny Hop disabled for {targetPlayer.PlayerName}");
            targetPlayer.PrintToChat($"Bunny Hop disabled for {targetPlayer.PlayerName}");
        }
    }

    public void OnTick()
    {
        var players = Utilities.GetPlayers();
        
        foreach (var player in players)
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive)
                continue;

            if (_bunnyHopEnabled.TryGetValue(player.SteamID, out var bunnyHopEnabled) && bunnyHopEnabled)
            {
                var playerPawn = player.PlayerPawn.Value;
                if (playerPawn != null)
                {
                    var flags = (PlayerFlags)playerPawn.Flags;
                    var buttons = player.Buttons;

                    if ((buttons & PlayerButtons.Jump) != 0)
                    {
                        if ((flags & PlayerFlags.FL_ONGROUND) != 0)
                        {
                            playerPawn.AbsVelocity.Z = 300; // Jump velocity
                        }
                    }
                }
            }
        }
    }
}
