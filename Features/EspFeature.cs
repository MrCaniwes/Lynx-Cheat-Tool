using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;
using LynxPlugin.Extensions;
using System.Drawing;
using LynxCheat;

namespace LynxPlugin.Features;

public class EspFeature : IFeature
{
    public string Name => "Lynx Professional ESP";
    
    private readonly HashSet<uint> _playersWithEsp = new();
    private readonly Dictionary<uint, (CDynamicProp relay, CDynamicProp glow)> _playerGlows = new();

    public HashSet<uint> PlayersWithEsp => _playersWithEsp;
    public Dictionary<uint, (CDynamicProp relay, CDynamicProp glow)> PlayerGlows => _playerGlows;

    private LynxCheat.LynxPlugin? _plugin;
    private string Prefix => _plugin?.Config.Prefix ?? "";

    public void OnLoad(BasePlugin plugin)
    {
        _plugin = (LynxCheat.LynxPlugin)plugin;

        _plugin.AddCommand($"css_{_plugin.Config.EspCommand}", "Toggle ESP for targets", (player, info) =>
        {
            if (player == null) return;
            if (!IsAuthorized(player)) { player.PrintToChat(_plugin.Localizer["esp.unauthorized", Prefix].ToString().ReplaceColorTags()); return; }

            string targetArg = info.GetArg(1);

            if (string.IsNullOrEmpty(targetArg))
            {
                ToggleEsp(player);
                return;
            }

            var targets = GetTargetPlayers(targetArg);
            if (targets.Count == 0)
            {
                player.PrintToChat(_plugin.Localizer["esp.target_not_found", Prefix, targetArg].ToString().ReplaceColorTags());
                return;
            }

            foreach (var target in targets) { ToggleEsp(target); }
            player.PrintToChat(_plugin.Localizer["esp.target_updated", Prefix, targetArg, targets.Count].ToString().ReplaceColorTags());
        });

        _plugin.RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid) return HookResult.Continue;
            if (_playersWithEsp.Count > 0)
            {
                _plugin.AddTimer(0.2f, () => {
                    if (player != null && player.IsValid && player.PawnIsAlive)
                    {
                        RemoveGlow((uint)player.Slot);
                        CreateGlow(player);
                    }
                });
            }
            return HookResult.Continue;
        });

        _plugin.RegisterEventHandler<EventPlayerDeath>((@event, info) => { var player = @event.Userid; if (player != null) RemoveGlow((uint)player.Slot); return HookResult.Continue; });
        _plugin.RegisterEventHandler<EventPlayerDisconnect>((@event, info) => { var player = @event.Userid; if (player != null) { _playersWithEsp.Remove((uint)player.Slot); RemoveGlow((uint)player.Slot); } return HookResult.Continue; });
        // Raunt başında her oyuncu zaten EventPlayerSpawn tetikleyeceği için toplu yenilemeye gerek yok.
    }

    private void ToggleEsp(CCSPlayerController player)
    {
        if (player == null || !player.IsValid || player.IsBot) return;
        uint slot = (uint)player.Slot;
        
        if (!_playersWithEsp.Contains(slot)) 
        { 
            _playersWithEsp.Add(slot); 
            player.PrintToChat(_plugin.Localizer["esp.enabled", Prefix].ToString().ReplaceColorTags()); 
            
            // Eğer bu WH açan İLK admin ise her şeyi yükle, değilse yüklenmiş olanları kullanırız.
            if (_playersWithEsp.Count == 1)
            {
                RefreshAllEntities(); 
            }
        }
        else 
        { 
            _playersWithEsp.Remove(slot); 
            player.PrintToChat(_plugin.Localizer["esp.disabled", Prefix].ToString().ReplaceColorTags()); 
            
            // Eğer ESP'si açık HİÇ KİMSE kalmadıysa temizlik yap.
            if (_playersWithEsp.Count == 0) 
            {
                RemoveAllEntities(); 
            }
        }
    }

    private List<CCSPlayerController> GetTargetPlayers(string arg)
    {
        var allPlayers = Utilities.GetPlayers().Where(p => p != null && p.IsValid).ToList();

        if (arg.StartsWith("#") && int.TryParse(arg.Substring(1), out int targetId))
        {
            var found = allPlayers.FirstOrDefault(p => p.Slot == targetId || p.UserId == targetId);
            if (found != null) return new List<CCSPlayerController> { found };
        }

        if (arg.Equals("@all", StringComparison.OrdinalIgnoreCase)) return allPlayers.Where(p => !p.IsBot).ToList();
        if (arg.Equals("@ct", StringComparison.OrdinalIgnoreCase)) return allPlayers.Where(p => p.TeamNum == (byte)CsTeam.CounterTerrorist).ToList();
        if (arg.Equals("@t", StringComparison.OrdinalIgnoreCase)) return allPlayers.Where(p => p.TeamNum == (byte)CsTeam.Terrorist).ToList();
        return allPlayers.Where(p => p.PlayerName.Contains(arg, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private bool IsAuthorized(CCSPlayerController player)
    {
        if (_plugin == null) return false;
        var config = _plugin.Config;

        // 1. SteamID Kontrolü
        if (config.AllowedSteamIds.Contains(player.SteamID.ToString())) 
            return true;

        // 2. Yetki (Flag) Kontrolü
        foreach (var flag in config.AllowedFlags)
        {
            if (AdminManager.PlayerHasPermissions(player, flag)) 
                return true;
        }

        // 3. Grup Kontrolü
        var adminData = AdminManager.GetPlayerAdminData(player);
        if (adminData != null)
        {
            foreach (var group in config.AllowedGroups)
            {
                if (adminData.Groups.Contains(group)) 
                    return true;
            }
        }

        return false;
    }

    public void CreateGlow(CCSPlayerController player)
    {
        var pawn = player.PlayerPawn.Value; if (pawn == null || !pawn.IsValid) return;
        string modelName = pawn.CBodyComponent?.SceneNode?.GetSkeletonInstance().ModelState.ModelName ?? ""; if (string.IsNullOrEmpty(modelName)) return;
        var relay = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        if (relay == null) return;
        relay.DispatchSpawn(); relay.SetModel(modelName); relay.Spawnflags = 256u; relay.RenderMode = RenderMode_t.kRenderNone;
        var glow = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        if (glow == null) { relay.Remove(); return; }
        glow.Render = Color.FromArgb(1, 0, 0, 0); glow.DispatchSpawn(); glow.SetModel(modelName); glow.Spawnflags = 256u;
        glow.Glow.GlowColorOverride = ConvertRgbToColor(player.TeamNum == (byte)CsTeam.CounterTerrorist ? _plugin!.Config.EspColorCt : _plugin!.Config.EspColorT);
        glow.Glow.GlowRange = (int)5000; glow.Glow.GlowTeam = -1; glow.Glow.GlowType = (int)_plugin!.Config.GlowType; glow.Glow.GlowRangeMin = (int)100;
        relay.AcceptInput("FollowEntity", pawn, relay, "!activator", 0); 
        glow.AcceptInput("FollowEntity", relay, glow, "!activator", 0);
        _playerGlows[(uint)player.Slot] = (relay, glow);
    }

    private Color ConvertRgbToColor(string rgb) { try { var parts = rgb.Split(' ', ','); if (parts.Length >= 3) { int r = int.Parse(parts[0]); int g = int.Parse(parts[1]); int b = int.Parse(parts[2]); int a = parts.Length > 3 ? int.Parse(parts[3]) : 255; return Color.FromArgb(a, r, g, b); } } catch { } return Color.White; }
    public void RefreshAllEntities() { RemoveAllEntities(); foreach (var player in Utilities.GetPlayers()) if (player != null && player.IsValid && player.PawnIsAlive) CreateGlow(player); }
    public void RemoveAllEntities() { var slots = _playerGlows.Keys.ToList(); foreach (var slot in slots) RemoveGlow(slot); }
    public void RemoveGlow(uint slot) { if (_playerGlows.TryGetValue(slot, out var models)) { if (models.relay != null && models.relay.IsValid) models.relay.Remove(); if (models.glow != null && models.glow.IsValid) models.glow.Remove(); _playerGlows.Remove(slot); } }

    public bool IsEnabled(CCSPlayerController player) => player != null && _playersWithEsp.Contains((uint)player.Slot);
}
