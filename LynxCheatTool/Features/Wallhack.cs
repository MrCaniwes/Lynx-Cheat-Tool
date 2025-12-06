using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager;
using CS2MenuManager.API.Menu;
using System.Drawing;

namespace LynxCheatTool.Features;

public class Wallhack
{
    private readonly LynxCheatTool _plugin;
    private readonly Dictionary<ulong, bool> _wallhackEnabled = new();
    private readonly Dictionary<CCSPlayerController, PlayerGlowData> _playerGlowData = new();

    private class PlayerGlowData
    {
        public CDynamicProp? ModelRelay { get; set; }
        public CDynamicProp? ModelGlow { get; set; }
        public string? ModelName { get; set; }
    }

    public Wallhack(LynxCheatTool plugin)
    {
        _plugin = plugin;
    }

    public void OnWallhackCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.WallhackPermission))
        {
            player.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Red}You do not have permission to use this command!{ChatColors.Default}");
            return;
        }

        ShowWallhackWasdMenu(player);
    }

    private void ShowWallhackWasdMenu(CCSPlayerController admin)
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

        WasdMenu menu = new(_plugin.Config.MenuTitle + " Wallhack Menu", _plugin);

        menu.AddItem("👥 Toggle All", (p, o) => 
        {
            ToggleWallhackAll(p);
            ShowWallhackWasdMenu(p);
        });

        foreach (var targetPlayer in allPlayers)
        {
            var steamId = targetPlayer.SteamID;
            var isEnabled = _wallhackEnabled.TryGetValue(steamId, out var enabled) && enabled;
            
            var statusIcon = isEnabled ? "✓" : "✗";
            var teamName = targetPlayer.TeamNum == 2 ? "[T]" : targetPlayer.TeamNum == 3 ? "[CT]" : "[SPEC]";
            var displayName = $"{statusIcon} {teamName} {targetPlayer.PlayerName}";

            menu.AddItem(displayName, (p, o) =>
            {
                ToggleWallhack(p, targetPlayer);
                
                ShowWallhackWasdMenu(p);
            });
        }

        menu.Display(admin, 30);
    }

    private void ToggleWallhackAll(CCSPlayerController admin)
    {
        var allPlayers = Utilities.GetPlayers().Where(p => p != null && p.IsValid).ToList();
        bool anyEnabled = allPlayers.Any(p => _wallhackEnabled.TryGetValue(p.SteamID, out var e) && e);
        bool newState = !anyEnabled;

        foreach (var player in allPlayers)
        {
            _wallhackEnabled[player.SteamID] = newState;
            
            if (!newState)
            {
                RemoveGlowEntity(player);
            }
        }

        string stateText = newState ? "Enabled" : "Disabled";
        admin.PrintToCenter($"All players Wallhack {stateText}");
    }

    private void ToggleWallhack(CCSPlayerController admin, CCSPlayerController targetPlayer)
    {
        var steamId = targetPlayer.SteamID;
        
        if (!_wallhackEnabled.ContainsKey(steamId))
            _wallhackEnabled[steamId] = false;

        _wallhackEnabled[steamId] = !_wallhackEnabled[steamId];

        if (_wallhackEnabled[steamId])
        {
            admin.PrintToCenter($"Wallhack enabled for {targetPlayer.PlayerName}");
            targetPlayer.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.LightPurple}Wallhack enabled!{ChatColors.Default} (Admin: {admin.PlayerName})");
        }
        else
        {
            admin.PrintToCenter($"Wallhack disabled for {targetPlayer.PlayerName}");
            targetPlayer.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Grey}Wallhack disabled.{ChatColors.Default}");
            RemoveGlowEntity(targetPlayer);
        }
    }

    public void OnTick()
    {
        var players = Utilities.GetPlayers();
        
        foreach (var player in players)
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive)
                continue;

            bool shouldGlow = false;
            foreach (var observer in players)
            {
                if (observer == null || !observer.IsValid)
                    continue;

                if (_wallhackEnabled.TryGetValue(observer.SteamID, out var enabled) && enabled)
                {
                    if (player.TeamNum != observer.TeamNum && player.Slot != observer.Slot)
                    {
                        shouldGlow = true;
                        break;
                    }
                }
            }

            if (shouldGlow)
            {
                UpdatePlayerGlow(player);
            }
            else
            {
                RemoveGlowEntity(player);
            }
        }
    }

    public void UpdateAllGlowColors()
    {
        foreach (var glowEntry in _playerGlowData)
        {
            var glowData = glowEntry.Value;
            if (glowData.ModelGlow != null && glowData.ModelGlow.IsValid)
            {
                glowData.ModelGlow.Glow.GlowColorOverride = Color.FromArgb(255, _plugin.Config.WallhackColorR, _plugin.Config.WallhackColorG, _plugin.Config.WallhackColorB);
            }
        }
    }

    public void OnPlayerDisconnect(CCSPlayerController player)
    {
        _wallhackEnabled.Remove(player.SteamID);
        RemoveGlowEntity(player);
    }

    public void OnPlayerDeath(CCSPlayerController player)
    {
        RemoveGlowEntity(player);
    }

    private void UpdatePlayerGlow(CCSPlayerController player)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null || !playerPawn.IsValid)
            return;

        var modelName = playerPawn.CBodyComponent?.SceneNode?.GetSkeletonInstance()?.ModelState.ModelName;
        if (string.IsNullOrEmpty(modelName))
            return;

        if (!_playerGlowData.TryGetValue(player, out var glowData))
        {
            glowData = new PlayerGlowData();
            _playerGlowData[player] = glowData;
        }

        if (glowData.ModelGlow == null || !glowData.ModelGlow.IsValid || glowData.ModelName != modelName)
        {
            RemoveGlowEntity(player);

            glowData.ModelRelay = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
            glowData.ModelGlow = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");

            if (glowData.ModelRelay == null || glowData.ModelGlow == null)
                return;

            glowData.ModelName = modelName;

            glowData.ModelRelay.SetModel(modelName);
            glowData.ModelRelay.Spawnflags = 256u;
            glowData.ModelRelay.Render = Color.FromArgb(0, 0, 0, 0); 
            glowData.ModelRelay.DispatchSpawn();

            glowData.ModelGlow.Render = Color.FromArgb(1, 0, 0, 0);
            glowData.ModelGlow.DispatchSpawn();
            glowData.ModelGlow.SetModel(modelName);
            glowData.ModelGlow.Spawnflags = 256u;

            glowData.ModelGlow.Glow.GlowColorOverride = Color.FromArgb(255, _plugin.Config.WallhackColorR, _plugin.Config.WallhackColorG, _plugin.Config.WallhackColorB);
            glowData.ModelGlow.Glow.GlowRange = 5000;
            glowData.ModelGlow.Glow.GlowTeam = -1;
            glowData.ModelGlow.Glow.GlowType = 3;
            glowData.ModelGlow.Glow.GlowRangeMin = 100;

            glowData.ModelRelay.AcceptInput("FollowEntity", playerPawn, glowData.ModelRelay, "!activator");
            glowData.ModelGlow.AcceptInput("FollowEntity", glowData.ModelRelay, glowData.ModelGlow, "!activator");

            _playerGlowData[player] = glowData;
        }
    }

    private void RemoveGlowEntity(CCSPlayerController player)
    {
        if (_playerGlowData.TryGetValue(player, out var glowData))
        {
            if (glowData.ModelGlow != null && glowData.ModelGlow.IsValid)
            {
                glowData.ModelGlow.AcceptInput("Kill");
            }

            if (glowData.ModelRelay != null && glowData.ModelRelay.IsValid)
            {
                glowData.ModelRelay.AcceptInput("Kill");
            }

            _playerGlowData.Remove(player);
        }
    }
}
