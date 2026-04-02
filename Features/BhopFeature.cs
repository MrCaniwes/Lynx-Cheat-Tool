using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using LynxPlugin.Extensions;
using LynxCheat;

namespace LynxPlugin.Features;

public class BhopFeature : IFeature
{
    public string Name => "Lynx Auto Bunny-Hop";
    private readonly HashSet<ulong> _playersWithBhop = new();
    private LynxCheat.LynxPlugin? _plugin;
    private string Prefix => _plugin?.Config.Prefix.ReplaceColorTags() ?? "";

    public void OnLoad(BasePlugin plugin)
    {
        _plugin = (LynxCheat.LynxPlugin)plugin;

        _plugin.AddCommand($"css_{_plugin.Config.BhopCommand}", "Toggle Auto Bunny-Hop", (player, info) =>
        {
            if (player == null) return;
            if (!IsAuthorized(player)) { player.PrintToChat(_plugin.Localizer["bhop.unauthorized", Prefix].ToString().ReplaceColorTags()); return; }

            string targetArg = info.GetArg(1);

            if (string.IsNullOrEmpty(targetArg))
            {
                ToggleBhop(player);
                return;
            }

            var targets = GetTargetPlayers(targetArg);
            if (targets.Count == 0)
            {
                player.PrintToChat(_plugin.Localizer["esp.target_not_found", Prefix, targetArg].ToString().ReplaceColorTags());
                return;
            }

            foreach (var target in targets) { ToggleBhop(target); }
            player.PrintToChat(_plugin.Localizer["bhop.target_updated", Prefix, targetArg, targets.Count].ToString().ReplaceColorTags());
        });

        _plugin.RegisterListener<Listeners.OnTick>(() =>
        {
            if (_playersWithBhop.Count == 0) return;

            foreach (var steamId in _playersWithBhop)
            {
                var player = Utilities.GetPlayers().FirstOrDefault(p => p != null && p.IsValid && p.SteamID == steamId);
                if (player == null || !player.PawnIsAlive) continue;

                var pawn = player.PlayerPawn.Value;
                if (pawn == null) continue;

                bool onGround = (pawn.Flags & (uint)PlayerFlags.FL_ONGROUND) != 0;
                bool isJumping = (player.Buttons & PlayerButtons.Jump) != 0;

                if (isJumping && onGround && pawn.MoveType != MoveType_t.MOVETYPE_LADDER)
                {
                    pawn.AbsVelocity.Z = 300.0f;
                }
            }
        });

        _plugin.RegisterEventHandler<EventPlayerDisconnect>((@event, info) => { var player = @event.Userid; if (player != null) _playersWithBhop.Remove(player.SteamID); return HookResult.Continue; });
    }

    private void ToggleBhop(CCSPlayerController player)
    {
        if (player == null || !player.IsValid || player.IsBot) return;
        if (!_playersWithBhop.Contains(player.SteamID)) { _playersWithBhop.Add(player.SteamID); player.PrintToChat(_plugin.Localizer["bhop.enabled", Prefix].ToString().ReplaceColorTags()); }
        else { _playersWithBhop.Remove(player.SteamID); player.PrintToChat(_plugin.Localizer["bhop.disabled", Prefix].ToString().ReplaceColorTags()); }
    }

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

    public bool IsEnabled(CCSPlayerController player) => player != null && _playersWithBhop.Contains(player.SteamID);
}
