using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using System.Numerics;
using LynxPlugin.Extensions;
using LynxCheat;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace LynxPlugin.Features;

public class AimbotFeature : IFeature
{
    public string Name => "Lynx Aimbot System";
    private readonly HashSet<ulong> _playersWithAimbot = new();
    private readonly Dictionary<ulong, CCSPlayerController?> _currentTarget = new();
    private LynxCheat.LynxPlugin? _plugin;
    private string Prefix => _plugin?.Config.Prefix.ReplaceColorTags() ?? "";

    public void OnLoad(BasePlugin plugin)
    {
        _plugin = (LynxCheat.LynxPlugin)plugin;

        _plugin.AddCommand($"css_{_plugin.Config.AimbotCommand}", "Toggle Aimbot for targets", (player, info) =>
        {
            if (player == null) return;
            if (!IsAuthorized(player)) { player.PrintToChat(_plugin.Localizer["aimbot.unauthorized", Prefix].ToString().ReplaceColorTags()); return; }

            string targetArg = info.GetArg(1);

            if (string.IsNullOrEmpty(targetArg))
            {
                ToggleAimbot(player);
                return;
            }

            var targets = GetTargetPlayers(targetArg);
            if (targets.Count == 0)
            {
                player.PrintToChat(_plugin.Localizer["esp.target_not_found", Prefix, targetArg].ToString().ReplaceColorTags());
                return;
            }

            foreach (var target in targets) { ToggleAimbot(target); }
            player.PrintToChat(_plugin.Localizer["aimbot.target_updated", Prefix, targetArg, targets.Count].ToString().ReplaceColorTags());
        });

        // FOV Değiştirme Komutu
        _plugin.AddCommand($"css_{_plugin.Config.AimbotFovCommand}", "Change Aimbot FOV", (player, info) =>
        {
            if (player == null || !IsAuthorized(player)) return;
            if (float.TryParse(info.GetArg(1), out float val)) { _plugin.Config.AimbotFOV = val; player.PrintToChat($"{Prefix} {ChatColors.Green}Aimbot FOV: {ChatColors.White}{val}".ReplaceColorTags()); }
        });

        // Smooth Değiştirme Komutu
        _plugin.AddCommand($"css_{_plugin.Config.AimbotSmoothCommand}", "Change Aimbot Smooth", (player, info) =>
        {
            if (player == null || !IsAuthorized(player)) return;
            if (float.TryParse(info.GetArg(1), out float val)) { _plugin.Config.AimbotSmooth = val; player.PrintToChat($"{Prefix} {ChatColors.Green}Aimbot Smooth: {ChatColors.White}{val}".ReplaceColorTags()); }
        });

        // Sticky FOV Değiştirme Komutu
        _plugin.AddCommand($"css_{_plugin.Config.AimbotStickyFovCommand}", "Change Aimbot Sticky FOV", (player, info) =>
        {
            if (player == null || !IsAuthorized(player)) return;
            if (float.TryParse(info.GetArg(1), out float val)) { _plugin.Config.AimbotStickyFOV = val; player.PrintToChat($"{Prefix} {ChatColors.Green}Aimbot Sticky FOV: {ChatColors.White}{val}".ReplaceColorTags()); }
        });

        _plugin.RegisterListener<Listeners.OnTick>(() =>
        {
            if (_playersWithAimbot.Count == 0) return;

            foreach (var steamId in _playersWithAimbot)
            {
                var player = Utilities.GetPlayers().FirstOrDefault(p => p != null && p.IsValid && p.SteamID == steamId);
                if (player == null || !player.PawnIsAlive) continue;

                var target = FindNearestEnemy(player);
                if (target != null)
                {
                    AimAtTarget(player, target);
                }
            }
        });

        _plugin.RegisterEventHandler<EventPlayerDisconnect>((@event, info) => { var player = @event.Userid; if (player != null) { _playersWithAimbot.Remove(player.SteamID); _currentTarget.Remove(player.SteamID); } return HookResult.Continue; });
    }

    private void ToggleAimbot(CCSPlayerController player)
    {
        if (player == null || !player.IsValid || player.IsBot) return;
        if (!_playersWithAimbot.Contains(player.SteamID)) { _playersWithAimbot.Add(player.SteamID); player.PrintToChat(_plugin.Localizer["aimbot.enabled", Prefix].ToString().ReplaceColorTags()); }
        else { _playersWithAimbot.Remove(player.SteamID); player.PrintToChat(_plugin.Localizer["aimbot.disabled", Prefix].ToString().ReplaceColorTags()); _currentTarget.Remove(player.SteamID); }
    }

    private CCSPlayerController? FindNearestEnemy(CCSPlayerController player)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null) return null;

        var playerPos = playerPawn.AbsOrigin;
        var playerEyeAngles = playerPawn.EyeAngles;
        if (playerPos == null || playerEyeAngles == null) return null;

        ulong steamId = player.SteamID;

        // Sticky FOV (Mevcut hedefi tutma)
        if (_currentTarget.TryGetValue(steamId, out var currentTarget) && currentTarget != null)
        {
            if (currentTarget.IsValid && currentTarget.PawnIsAlive && currentTarget.TeamNum != player.TeamNum)
            {
                var targetPawn = currentTarget.PlayerPawn.Value;
                if (targetPawn != null && targetPawn.AbsOrigin != null)
                {
                    var angleToCurrent = CalculateAngle(playerPos, targetPawn.AbsOrigin, playerPawn);
                    var fovDiff = GetFovDifference(playerEyeAngles, angleToCurrent);

                    if (fovDiff <= (_plugin?.Config.AimbotStickyFOV ?? 25.0f)) return currentTarget;
                }
            }
            _currentTarget[steamId] = null;
        }

        CCSPlayerController? nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        var players = Utilities.GetPlayers();
        foreach (var target in players)
        {
            if (target == null || !target.IsValid || !target.PawnIsAlive || target.TeamNum == player.TeamNum || target.Slot == player.Slot) continue;

            var targetPawn = target.PlayerPawn.Value;
            if (targetPawn == null || targetPawn.AbsOrigin == null) continue;

            var distance = Vector3.Distance(new Vector3(playerPos.X, playerPos.Y, playerPos.Z), new Vector3(targetPawn.AbsOrigin.X, targetPawn.AbsOrigin.Y, targetPawn.AbsOrigin.Z));
            var angleToTarget = CalculateAngle(playerPos, targetPawn.AbsOrigin, playerPawn);
            var fovDiff = GetFovDifference(playerEyeAngles, angleToTarget);

            if (fovDiff > (_plugin?.Config.AimbotFOV ?? 15.0f)) continue;

            if (distance < nearestDistance) { nearestDistance = distance; nearestEnemy = target; }
        }

        if (nearestEnemy != null) _currentTarget[steamId] = nearestEnemy;
        return nearestEnemy;
    }

    private void AimAtTarget(CCSPlayerController player, CCSPlayerController target)
    {
        var playerPawn = player.PlayerPawn.Value;
        var targetPawn = target.PlayerPawn.Value;
        if (playerPawn == null || targetPawn == null || playerPawn.AbsOrigin == null || targetPawn.AbsOrigin == null) return;

        bool isCrouched = (targetPawn.Flags & (uint)PlayerFlags.FL_DUCKING) != 0;
        float headHeight = isCrouched ? 46.0f : 64.0f;
        
        Vector targetHeadPos = new Vector(targetPawn.AbsOrigin.X, targetPawn.AbsOrigin.Y, targetPawn.AbsOrigin.Z + headHeight);
        QAngle targetAngle = CalculateAngle(playerPawn.AbsOrigin, targetHeadPos, playerPawn);

        if (playerPawn.EyeAngles == null) return;

        QAngle newAngle = new QAngle(
            Lerp(playerPawn.EyeAngles.X, targetAngle.X, _plugin?.Config.AimbotSmooth ?? 0.1f),
            Lerp(playerPawn.EyeAngles.Y, targetAngle.Y, _plugin?.Config.AimbotSmooth ?? 0.1f),
            playerPawn.EyeAngles.Z
        );

        newAngle.X = NormalizeAngle(newAngle.X);
        newAngle.Y = NormalizeAngle(newAngle.Y);

        playerPawn.Teleport(playerPawn.AbsOrigin, newAngle, playerPawn.AbsVelocity);
    }

    private QAngle CalculateAngle(Vector from, Vector to, CCSPlayerPawn pawn)
    {
        Vector eyePos = new Vector(from.X, from.Y, from.Z + 64.0f);
        Vector3 delta = new Vector3(to.X - eyePos.X, to.Y - eyePos.Y, to.Z - eyePos.Z);
        double hyp = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
        return new QAngle((float)(Math.Atan2(-delta.Z, hyp) * (180.0 / Math.PI)), (float)(Math.Atan2(delta.Y, delta.X) * (180.0 / Math.PI)), 0);
    }

    private float GetFovDifference(QAngle current, QAngle target)
    {
        float diffX = Math.Abs(NormalizeAngle(current.X - target.X));
        float diffY = Math.Abs(NormalizeAngle(current.Y - target.Y));
        return (float)Math.Sqrt(diffX * diffX + diffY * diffY);
    }

    private float NormalizeAngle(float angle) { while (angle > 180) angle -= 360; while (angle < -180) angle += 360; return angle; }
    private float Lerp(float a, float b, float t) { return a + (b - a) * t; }

    private List<CCSPlayerController> GetTargetPlayers(string arg)
    {
        var allPlayers = Utilities.GetPlayers().Where(p => p != null && p.IsValid).ToList();
        if (arg.StartsWith("#") && int.TryParse(arg.Substring(1), out int tid)) { var f = allPlayers.FirstOrDefault(p => p.Slot == tid || p.UserId == tid); if (f != null) return new List<CCSPlayerController> { f }; }
        if (arg.Equals("@all", StringComparison.OrdinalIgnoreCase)) return allPlayers.Where(p => !p.IsBot).ToList();
        if (arg.Equals("@ct", StringComparison.OrdinalIgnoreCase)) return allPlayers.Where(p => p.TeamNum == (byte)CsTeam.CounterTerrorist).ToList();
        if (arg.Equals("@t", StringComparison.OrdinalIgnoreCase)) return allPlayers.Where(p => p.TeamNum == (byte)CsTeam.Terrorist).ToList();
        return allPlayers.Where(p => p.PlayerName.Contains(arg, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private bool IsAuthorized(CCSPlayerController player)
    {
        if (_plugin == null) return false;
        var config = _plugin.Config;
        if (config.AllowedSteamIds.Contains(player.SteamID.ToString())) return true;
        foreach (var flag in config.AllowedFlags) if (AdminManager.PlayerHasPermissions(player, flag)) return true;
        var adminData = AdminManager.GetPlayerAdminData(player);
        if (adminData != null) foreach (var group in config.AllowedGroups) if (adminData.Groups.Contains(group)) return true;
        return false;
    }

    public bool IsEnabled(CCSPlayerController player) => player != null && _playersWithAimbot.Contains(player.SteamID);
}
