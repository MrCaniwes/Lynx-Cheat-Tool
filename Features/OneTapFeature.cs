using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using LynxPlugin.Extensions;
using LynxCheat;

namespace LynxPlugin.Features;

public class OneTapFeature : IFeature
{
    public string Name => "Lynx One-Tap Kill";
    private readonly HashSet<ulong> _playersWithOneTap = new();
    private LynxCheat.LynxPlugin? _plugin;
    private string Prefix => _plugin?.Config.Prefix.ReplaceColorTags() ?? "";

    public void OnLoad(BasePlugin plugin)
    {
        _plugin = (LynxCheat.LynxPlugin)plugin;

        _plugin.AddCommand($"css_{_plugin.Config.OneTapCommand}", "Toggle One-Tap kill", (player, info) =>
        {
            if (player == null) return;
            if (!IsAuthorized(player)) { player.PrintToChat(_plugin.Localizer["onetap.unauthorized", Prefix].ToString().ReplaceColorTags()); return; }

            string targetArg = info.GetArg(1);

            if (string.IsNullOrEmpty(targetArg))
            {
                ToggleOneTap(player);
                return;
            }

            var targets = GetTargetPlayers(targetArg);
            if (targets.Count == 0)
            {
                player.PrintToChat(_plugin.Localizer["esp.target_not_found", Prefix, targetArg].ToString().ReplaceColorTags());
                return;
            }

            foreach (var target in targets) { ToggleOneTap(target); }
            player.PrintToChat(_plugin.Localizer["onetap.target_updated", Prefix, targetArg, targets.Count].ToString().ReplaceColorTags());
        });

        _plugin.RegisterEventHandler<EventPlayerHurt>((@event, info) =>
        {
            var attacker = @event.Attacker;
            var victim = @event.Userid;

            if (attacker == null || !attacker.IsValid || victim == null || !victim.IsValid) return HookResult.Continue;
            if (attacker.TeamNum == victim.TeamNum) return HookResult.Continue;

            if (_playersWithOneTap.Contains(attacker.SteamID))
            {
                var pawn = victim.PlayerPawn.Value;
                if (pawn != null && pawn.IsValid)
                {
                    pawn.Health = 0;
                    Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
                }
            }
            return HookResult.Continue;
        });

        _plugin.RegisterEventHandler<EventPlayerDeath>((@event, info) =>
        {
            var attacker = @event.Attacker;
            if (attacker != null && attacker.IsValid && _playersWithOneTap.Contains(attacker.SteamID))
            {
                @event.Headshot = true;
                return HookResult.Changed;
            }
            return HookResult.Continue;
        });

        _plugin.RegisterEventHandler<EventPlayerDisconnect>((@event, info) => { var player = @event.Userid; if (player != null) _playersWithOneTap.Remove(player.SteamID); return HookResult.Continue; });
    }

    private void ToggleOneTap(CCSPlayerController player)
    {
        if (player == null || !player.IsValid || player.IsBot) return;
        if (!_playersWithOneTap.Contains(player.SteamID)) { _playersWithOneTap.Add(player.SteamID); player.PrintToChat(_plugin.Localizer["onetap.enabled", Prefix].ToString().ReplaceColorTags()); }
        else { _playersWithOneTap.Remove(player.SteamID); player.PrintToChat(_plugin.Localizer["onetap.disabled", Prefix].ToString().ReplaceColorTags()); }
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

    public bool IsEnabled(CCSPlayerController player) => player != null && _playersWithOneTap.Contains(player.SteamID);
}
