using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager;
using CS2MenuManager.API.Menu;

namespace LynxCheatTool.Features;

public class MagicBullet
{
    private readonly LynxCheatTool _plugin;
    private readonly Dictionary<ulong, bool> _magicBulletEnabled = new();

    public MagicBullet(LynxCheatTool plugin)
    {
        _plugin = plugin;
    }

    public void OnMagicBulletCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.MagicBulletPermission))
        {
            player.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Red}You do not have permission to use this command!{ChatColors.Default}");
            return;
        }

        ShowMagicBulletWasdMenu(player);
    }

    private void ShowMagicBulletWasdMenu(CCSPlayerController admin)
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

        WasdMenu menu = new(_plugin.Config.MenuTitle + " Magic Bullet Menu", _plugin);

        menu.AddItem("👥 Toggle All", (p, o) => 
        {
            ToggleMagicBulletAll(p);
            ShowMagicBulletWasdMenu(p);
        });

        foreach (var targetPlayer in allPlayers)
        {
            var steamId = targetPlayer.SteamID;
            var isEnabled = _magicBulletEnabled.TryGetValue(steamId, out var enabled) && enabled;
            
            var statusIcon = isEnabled ? "✓" : "✗";
            var teamName = targetPlayer.TeamNum == 2 ? "[T]" : targetPlayer.TeamNum == 3 ? "[CT]" : "[SPEC]";
            var displayName = $"{statusIcon} {teamName} {targetPlayer.PlayerName}";

            menu.AddItem(displayName, (p, o) =>
            {
                ToggleMagicBullet(p, targetPlayer);
                
                ShowMagicBulletWasdMenu(p);
            });
        }

        menu.Display(admin, 30);
    }

    private void ToggleMagicBulletAll(CCSPlayerController admin)
    {
        var allPlayers = Utilities.GetPlayers().Where(p => p != null && p.IsValid).ToList();
        bool anyEnabled = allPlayers.Any(p => _magicBulletEnabled.TryGetValue(p.SteamID, out var e) && e);
        bool newState = !anyEnabled;

        foreach (var player in allPlayers)
        {
            _magicBulletEnabled[player.SteamID] = newState;
        }

        string stateText = newState ? "Enabled" : "Disabled";
        admin.PrintToCenter($"All players Magic Bullet {stateText}");
    }

    private void ToggleMagicBullet(CCSPlayerController admin, CCSPlayerController targetPlayer)
    {
        var steamId = targetPlayer.SteamID;
        
        if (!_magicBulletEnabled.ContainsKey(steamId))
            _magicBulletEnabled[steamId] = false;

        _magicBulletEnabled[steamId] = !_magicBulletEnabled[steamId];

        if (_magicBulletEnabled[steamId])
        {
            admin.PrintToCenter($"Magic Bullet enabled for {targetPlayer.PlayerName}");
            targetPlayer.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.LightRed}Magic Bullet enabled! Tek atış!{ChatColors.Default} (Admin: {admin.PlayerName})");
        }
        else
        {
            admin.PrintToCenter($"Magic Bullet disabled for {targetPlayer.PlayerName}");
            targetPlayer.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Grey}Magic Bullet disabled.{ChatColors.Default}");
        }
    }

    public void OnPlayerHurt(EventPlayerHurt @event)
    {
        if (@event.Attacker == null || @event.Userid == null)
            return;

        var attacker = @event.Attacker;
        var victim = @event.Userid;

        if (attacker.IsValid && _magicBulletEnabled.TryGetValue(attacker.SteamID, out var enabled) && enabled)
        {
            if (victim.IsValid && victim.PawnIsAlive && victim.TeamNum != attacker.TeamNum)
            {
                if (victim.PlayerPawn.Value != null)
                {
                    victim.PlayerPawn.Value.Health = 0;
                }
            }
        }
    }

    public HookResult OnPlayerDeath(EventPlayerDeath @event)
    {
        if (@event?.Attacker != null && @event.Attacker.IsValid)
        {
            var attackerSteamId = @event.Attacker.SteamID;

            if (_magicBulletEnabled.TryGetValue(attackerSteamId, out var enabled) && enabled)
            {
                @event.Headshot = true;
                return HookResult.Changed;
            }
        }
        return HookResult.Continue;
    }
}
