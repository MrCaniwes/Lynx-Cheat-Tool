using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using System.Drawing;
using LynxPlugin.Extensions;
using LynxCheat;

namespace LynxPlugin.Features;

public class InvisibilityFeature : IFeature
{
    public string Name => "Lynx Invisibility System";
    private readonly HashSet<ulong> _playersWithInvis = new();
    private LynxCheat.LynxPlugin? _plugin;
    private string Prefix => _plugin?.Config.Prefix.ReplaceColorTags() ?? "";

    public void OnLoad(BasePlugin plugin)
    {
        _plugin = (LynxCheat.LynxPlugin)plugin;

        _plugin.AddCommand($"css_{_plugin.Config.InvisibilityCommand}", "Toggle Invisibility for targets", (player, info) =>
        {
            if (player == null) return;
            if (!IsAuthorized(player)) { player.PrintToChat(_plugin.Localizer["inv.unauthorized", Prefix].ToString().ReplaceColorTags()); return; }

            string targetArg = info.GetArg(1);

            if (string.IsNullOrEmpty(targetArg))
            {
                ToggleInvis(player);
                return;
            }

            var targets = GetTargetPlayers(targetArg);
            if (targets.Count == 0)
            {
                player.PrintToChat(_plugin.Localizer["esp.target_not_found", Prefix, targetArg].ToString().ReplaceColorTags());
                return;
            }

            foreach (var target in targets) { ToggleInvis(target); }
            player.PrintToChat(_plugin.Localizer["inv.target_updated", Prefix, targetArg, targets.Count].ToString().ReplaceColorTags());
        });

        _plugin.RegisterListener<Listeners.OnTick>(() =>
        {
            if (_playersWithInvis.Count == 0) return;

            foreach (var steamId in _playersWithInvis)
            {
                var player = Utilities.GetPlayers().FirstOrDefault(p => p != null && p.IsValid && p.SteamID == steamId);
                if (player == null || !player.PawnIsAlive) continue;

                var pawn = player.PlayerPawn.Value;
                if (pawn == null) continue;

                // Oyuncuyu ve silahları Alpha 0 (Görünmez) yapıyoruz
                pawn.Render = Color.FromArgb(0, pawn.Render);
                Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

                if (pawn.WeaponServices != null)
                {
                    foreach (var weaponHandle in pawn.WeaponServices.MyWeapons)
                    {
                        var weapon = weaponHandle.Value;
                        if (weapon != null && weapon.IsValid)
                        {
                            weapon.Render = pawn.Render;
                            Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");
                        }
                    }
                }
            }
        });

        _plugin.RegisterEventHandler<EventPlayerDisconnect>((@event, info) => { var player = @event.Userid; if (player != null) _playersWithInvis.Remove(player.SteamID); return HookResult.Continue; });
    }

    private void ToggleInvis(CCSPlayerController player)
    {
        if (player == null || !player.IsValid || player.IsBot) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        if (!_playersWithInvis.Contains(player.SteamID)) 
        { 
            _playersWithInvis.Add(player.SteamID); 
            player.PrintToChat(_plugin.Localizer["inv.enabled", Prefix].ToString().ReplaceColorTags()); 
        }
        else 
        { 
            _playersWithInvis.Remove(player.SteamID); 
            
            // Görünürlüğü geri getirme (Alpha 255)
            pawn.Render = Color.FromArgb(255, pawn.Render);
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

            if (pawn.WeaponServices != null)
            {
                foreach (var weaponHandle in pawn.WeaponServices.MyWeapons)
                {
                    var weapon = weaponHandle.Value;
                    if (weapon != null && weapon.IsValid)
                    {
                        weapon.Render = pawn.Render;
                        Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");
                    }
                }
            }

            player.PrintToChat(_plugin.Localizer["inv.disabled", Prefix].ToString().ReplaceColorTags());
        }
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

    public bool IsEnabled(CCSPlayerController player) => player != null && _playersWithInvis.Contains(player.SteamID);
}
