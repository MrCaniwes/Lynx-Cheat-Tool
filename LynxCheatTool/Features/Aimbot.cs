using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager;
using CS2MenuManager.API.Menu;
using System.Numerics;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace LynxCheatTool.Features;

public class Aimbot
{
    private readonly LynxCheatTool _plugin;
    private readonly Dictionary<ulong, bool> _aimbotEnabled = new();
    private readonly Dictionary<ulong, CCSPlayerController?> _currentTarget = new();

    public Aimbot(LynxCheatTool plugin)
    {
        _plugin = plugin;
    }

    public void OnAimbotCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.AimbotPermission))
        {
            player.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Red}You do not have permission to use this command!{ChatColors.Default}");
            return;
        }

        ShowAimbotWasdMenu(player);
    }

    private void ShowAimbotWasdMenu(CCSPlayerController admin)
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

        WasdMenu menu = new(_plugin.Config.MenuTitle + " Aimbot Menu", _plugin);

        menu.AddItem("👥 Toggle All", (p, o) => 
        {
            ToggleAimbotAll(p);
            ShowAimbotWasdMenu(p);
        });

        foreach (var targetPlayer in allPlayers)
        {
            var steamId = targetPlayer.SteamID;
            var isEnabled = _aimbotEnabled.TryGetValue(steamId, out var enabled) && enabled;
            
            var statusIcon = isEnabled ? "✓" : "✗";
            var teamName = targetPlayer.TeamNum == 2 ? "[T]" : targetPlayer.TeamNum == 3 ? "[CT]" : "[SPEC]";
            var displayName = $"{statusIcon} {teamName} {targetPlayer.PlayerName}";

            menu.AddItem(displayName, (p, o) =>
            {
                ToggleAimbot(p, targetPlayer);
                
                ShowAimbotWasdMenu(p);
            });
        }

        menu.Display(admin, 30);
    }

    private void ToggleAimbotAll(CCSPlayerController admin)
    {
        var allPlayers = Utilities.GetPlayers().Where(p => p != null && p.IsValid).ToList();
        bool anyEnabled = allPlayers.Any(p => _aimbotEnabled.TryGetValue(p.SteamID, out var e) && e);
        bool newState = !anyEnabled;

        foreach (var player in allPlayers)
        {
            _aimbotEnabled[player.SteamID] = newState;
        }

        string stateText = newState ? "Enabled" : "Disabled";
        admin.PrintToCenter($"All players Aimbot status changed to {stateText}");
    }

    private void ToggleAimbot(CCSPlayerController admin, CCSPlayerController targetPlayer)
    {
        var steamId = targetPlayer.SteamID;
        
        if (!_aimbotEnabled.ContainsKey(steamId))
            _aimbotEnabled[steamId] = false;

        _aimbotEnabled[steamId] = !_aimbotEnabled[steamId];

        if (_aimbotEnabled[steamId])
        {
            admin.PrintToCenter($"Aimbot enabled for {targetPlayer.PlayerName}");
            targetPlayer.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.LightRed}Aimbot enabled!{ChatColors.Default} (Admin: {admin.PlayerName})");
        }
        else
        {
            admin.PrintToCenter($"Aimbot disabled for {targetPlayer.PlayerName}");
            targetPlayer.PrintToChat($" {ChatColors.Green}{_plugin.Config.ChatTag}{ChatColors.Default} {ChatColors.Grey}Aimbot disabled.{ChatColors.Default}");
            
            if (_currentTarget.ContainsKey(steamId))
                _currentTarget[steamId] = null;
        }
    }

    public void OnTick()
    {
        var players = Utilities.GetPlayers();
        
        foreach (var player in players)
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive)
                continue;

            if (_aimbotEnabled.TryGetValue(player.SteamID, out var aimbotEnabled) && aimbotEnabled)
            {
                var playerPawn = player.PlayerPawn.Value;
                if (playerPawn != null)
                {
                    var target = FindNearestEnemy(player);
                    if (target != null)
                    {
                        AimAtTarget(player, target);
                    }
                }
            }
        }
    }

    private CCSPlayerController? FindNearestEnemy(CCSPlayerController player)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null)
            return null;

        var playerPos = playerPawn.AbsOrigin;
        if (playerPos == null)
            return null;

        var playerEyeAngles = playerPawn.EyeAngles;
        if (playerEyeAngles == null)
            return null;

        var steamId = player.SteamID;

        if (_currentTarget.TryGetValue(steamId, out var currentTarget) && currentTarget != null)
        {
            if (currentTarget.IsValid && currentTarget.PawnIsAlive && 
                currentTarget.TeamNum != player.TeamNum)
            {
                var currentTargetPawn = currentTarget.PlayerPawn.Value;
                if (currentTargetPawn != null && currentTargetPawn.AbsOrigin != null)
                {
                    var angleToCurrentTarget = CalculateAngle(playerPos, currentTargetPawn.AbsOrigin, playerPawn);
                    var fovDiffCurrent = GetFovDifference(playerEyeAngles, angleToCurrentTarget);

                    if (fovDiffCurrent <= _plugin.Config.AimbotStickyFOV)
                    {
                        return currentTarget;
                    }
                }
            }
            
            _currentTarget[steamId] = null;
        }

        CCSPlayerController? nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        var players = Utilities.GetPlayers();
        foreach (var target in players)
        {
            if (target == null || !target.IsValid || !target.PawnIsAlive)
                continue;

            if (target.TeamNum == player.TeamNum)
                continue;

            if (target.Slot == player.Slot)
                continue;

            var targetPawn = target.PlayerPawn.Value;
            if (targetPawn == null)
                continue;

            var targetPos = targetPawn.AbsOrigin;
            if (targetPos == null)
                continue;

            var distance = Vector3.Distance(
                new Vector3(playerPos.X, playerPos.Y, playerPos.Z),
                new Vector3(targetPos.X, targetPos.Y, targetPos.Z)
            );

            var angleToTarget = CalculateAngle(playerPos, targetPos, playerPawn);
            var fovDiff = GetFovDifference(playerEyeAngles, angleToTarget);

            if (fovDiff > _plugin.Config.AimbotFOV)
                continue;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = target;
            }
        }

        if (nearestEnemy != null)
        {
            _currentTarget[steamId] = nearestEnemy;
        }

        return nearestEnemy;
    }

    private void AimAtTarget(CCSPlayerController player, CCSPlayerController target)
    {
        var playerPawn = player.PlayerPawn.Value;
        var targetPawn = target.PlayerPawn.Value;

        if (playerPawn == null || targetPawn == null)
            return;

        var playerPos = playerPawn.AbsOrigin;
        var targetPos = targetPawn.AbsOrigin;

        if (playerPos == null || targetPos == null)
            return;

        Vector targetHeadPos;
        
        bool isCrouched = (targetPawn.Flags & (uint)PlayerFlags.FL_DUCKING) != 0;
        float headHeight = isCrouched ? 46.0f : 64.0f; 
        
        var sceneNode = targetPawn.CBodyComponent?.SceneNode;
        if (sceneNode != null)
        {
            try
            {
                var targetAngles = targetPawn.EyeAngles;
                if (targetAngles != null)
                {
                    var yawRad = targetAngles.Y * (Math.PI / 180.0);
                    var forwardOffset = 3.0f; 
                    
                    targetHeadPos = new Vector(
                        targetPos.X + (float)(Math.Cos(yawRad) * forwardOffset),
                        targetPos.Y + (float)(Math.Sin(yawRad) * forwardOffset),
                        targetPos.Z + headHeight 
                    );
                }
                else
                {
                    targetHeadPos = new Vector(targetPos.X, targetPos.Y, targetPos.Z + headHeight);
                }
            }
            catch
            {
                targetHeadPos = new Vector(targetPos.X, targetPos.Y, targetPos.Z + headHeight);
            }
        }
        else
        {
            targetHeadPos = new Vector(targetPos.X, targetPos.Y, targetPos.Z + headHeight);
        }

        var targetAngle = CalculateAngle(playerPos, targetHeadPos, playerPawn);

        var currentAngle = playerPawn.EyeAngles;
        if (currentAngle == null)
            return;

        var newAngle = new QAngle(
            Lerp(currentAngle.X, targetAngle.X, _plugin.Config.AimbotSmooth),
            Lerp(currentAngle.Y, targetAngle.Y, _plugin.Config.AimbotSmooth),
            currentAngle.Z
        );

        newAngle.X = NormalizeAngle(newAngle.X);
        newAngle.Y = NormalizeAngle(newAngle.Y);

        var currentVelocity = playerPawn.AbsVelocity;
        playerPawn.Teleport(playerPos, newAngle, currentVelocity);
        
        player.PrintToCenter($"🎯 Locked: {target.PlayerName}");
    }

    private QAngle CalculateAngle(Vector from, Vector to, CCSPlayerPawn pawn)
    {
        var eyePos = pawn.AbsOrigin;
        var eyeHeight = 64.0f; 
        
        if (eyePos != null)
        {
            eyePos = new Vector(eyePos.X, eyePos.Y, eyePos.Z + eyeHeight);
        }
        else
        {
            eyePos = from;
        }

        var delta = new Vector3(
            to.X - eyePos.X,
            to.Y - eyePos.Y,
            to.Z - eyePos.Z
        );

        var hyp = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
        
        var pitch = (float)(Math.Atan2(-delta.Z, hyp) * (180.0 / Math.PI));
        var yaw = (float)(Math.Atan2(delta.Y, delta.X) * (180.0 / Math.PI));

        return new QAngle(pitch, yaw, 0);
    }

    private float GetFovDifference(QAngle current, QAngle target)
    {
        var diffX = Math.Abs(NormalizeAngle(current.X - target.X));
        var diffY = Math.Abs(NormalizeAngle(current.Y - target.Y));
        
        return (float)Math.Sqrt(diffX * diffX + diffY * diffY);
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180)
            angle -= 360;
        while (angle < -180)
            angle += 360;
        return angle;
    }

    private float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}
