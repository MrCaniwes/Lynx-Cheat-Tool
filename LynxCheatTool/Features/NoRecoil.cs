using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager;
using CS2MenuManager.API.Menu;

namespace LynxCheatTool.Features;

public class NoRecoil
{
    private readonly LynxCheatTool _plugin;
    private readonly Dictionary<ulong, bool> _noRecoilEnabled = new();

    public NoRecoil(LynxCheatTool plugin)
    {
        _plugin = plugin;
    }

    public void OnNoRecoilCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.NoRecoilPermission))
        {
            player.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Red}You do not have permission to use this command!{ChatColors.Default}");
            return;
        }

        ShowNoRecoilWasdMenu(player);
    }

    private void ShowNoRecoilWasdMenu(CCSPlayerController admin)
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

        WasdMenu menu = new(_plugin.Config.MenuTitle + " No Recoil Menu", _plugin);

        menu.AddItem("👥 Toggle All", (p, o) => 
        {
            ToggleNoRecoilAll(p);
            ShowNoRecoilWasdMenu(p);
        });

        foreach (var targetPlayer in allPlayers)
        {
            var steamId = targetPlayer.SteamID;
            var isEnabled = _noRecoilEnabled.TryGetValue(steamId, out var enabled) && enabled;
            
            var statusIcon = isEnabled ? "✓" : "✗";
            var teamName = targetPlayer.TeamNum == 2 ? "[T]" : targetPlayer.TeamNum == 3 ? "[CT]" : "[SPEC]";
            var displayName = $"{statusIcon} {teamName} {targetPlayer.PlayerName}";

            menu.AddItem(displayName, (p, o) =>
            {
                ToggleNoRecoil(p, targetPlayer);
                ShowNoRecoilWasdMenu(p);
            });
        }

        menu.Display(admin, 30);
    }

    private void ToggleNoRecoilAll(CCSPlayerController admin)
    {
        var allPlayers = Utilities.GetPlayers().Where(p => p != null && p.IsValid).ToList();
        bool anyEnabled = allPlayers.Any(p => _noRecoilEnabled.TryGetValue(p.SteamID, out var e) && e);
        bool newState = !anyEnabled;

        foreach (var player in allPlayers)
        {
            _noRecoilEnabled[player.SteamID] = newState;
        }

        string stateText = newState ? "Enabled" : "Disabled";
        admin.PrintToCenter($"All players No Recoil {stateText}");
    }

    private void ToggleNoRecoil(CCSPlayerController admin, CCSPlayerController targetPlayer)
    {
        var steamId = targetPlayer.SteamID;
        
        if (!_noRecoilEnabled.ContainsKey(steamId))
            _noRecoilEnabled[steamId] = false;

        _noRecoilEnabled[steamId] = !_noRecoilEnabled[steamId];

        if (_noRecoilEnabled[steamId])
        {
            admin.PrintToCenter($"No Recoil enabled for {targetPlayer.PlayerName}");
            targetPlayer.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.LightBlue}No Recoil enabled! Laser beam!{ChatColors.Default} (Admin: {admin.PlayerName})");
        }
        else
        {
            admin.PrintToCenter($"No Recoil disabled for {targetPlayer.PlayerName}");
            targetPlayer.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Grey}No Recoil disabled.{ChatColors.Default}");
        }
    }

    public void OnTick()
    {
        var players = Utilities.GetPlayers();
        
        foreach (var player in players)
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive)
                continue;

            if (_noRecoilEnabled.TryGetValue(player.SteamID, out var noRecoilEnabled) && noRecoilEnabled)
            {
                var playerPawn = player.PlayerPawn.Value;
                if (playerPawn != null)
                {
                    playerPawn.ShotsFired = 0;

                    if (playerPawn.AimPunchAngle != null)
                    {
                        playerPawn.AimPunchAngle.X = 0;
                        playerPawn.AimPunchAngle.Y = 0;
                        playerPawn.AimPunchAngle.Z = 0;
                    }

                    if (playerPawn.AimPunchAngleVel != null)
                    {
                        playerPawn.AimPunchAngleVel.X = 0;
                        playerPawn.AimPunchAngleVel.Y = 0;
                        playerPawn.AimPunchAngleVel.Z = 0;
                    }
                }
            }
        }
    }
}
