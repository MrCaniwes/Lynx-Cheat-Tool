using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager;
using CS2MenuManager.API.Menu;
using LynxCheatTool.Features;
using System.Text.Json;

namespace LynxCheatTool;

public class LynxCheatTool : BasePlugin, IPluginConfig<LynxCheatToolConfig>
{
    public override string ModuleName => "LynxCheatTool";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "LynxHera";
    public override string ModuleDescription => "A comprehensive cheat tool plugin for CS2";

    public required LynxCheatToolConfig Config { get; set; }

    private Aimbot? _aimbot;
    private Wallhack? _wallhack;
    private MagicBullet? _magicBullet;
    private NoRecoil? _noRecoil;
    private NoFlash? _noFlash;
    private FragChanger? _fragChanger;
    private BunnyHop? _bunnyHop;

    public void OnConfigParsed(LynxCheatToolConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        

        // Initialize Features
        _aimbot = new Aimbot(this);
        _wallhack = new Wallhack(this);
        _magicBullet = new MagicBullet(this);
        _noRecoil = new NoRecoil(this);
        _noFlash = new NoFlash(this);
        _fragChanger = new FragChanger(this);
        _bunnyHop = new BunnyHop(this);

        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath, HookMode.Pre);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
        
        // Reload komutu sadece bir kez kaydedilir
        AddCommand("css_lynx_reload", "Reload LynxCheatTool configuration", OnReloadConfigCommand);
        
        // Dinamik komut kayıtları
        RegisterCommands();
        
        Console.WriteLine($"{Config.ChatTag} Plugin loaded!");
    }

    private void RegisterCommands()
    {
        if (_aimbot != null) AddCommand(Config.AimbotCommand, "Aimbot WASD menu", _aimbot.OnAimbotCommand);
        if (_wallhack != null) AddCommand(Config.WallhackCommand, "Wallhack WASD menu", _wallhack.OnWallhackCommand);
        if (_magicBullet != null) AddCommand(Config.MagicBulletCommand, "Magic Bullet menu", _magicBullet.OnMagicBulletCommand);
        if (_noRecoil != null) AddCommand(Config.NoRecoilCommand, "No Recoil WASD menu", _noRecoil.OnNoRecoilCommand);
        if (_noFlash != null) AddCommand(Config.NoFlashCommand, "No Flash WASD menu", _noFlash.OnNoFlashCommand);
        if (_fragChanger != null) AddCommand(Config.FragChangerCommand, "Frag Changer WASD menu", _fragChanger.OnFragChangerCommand);
        if (_bunnyHop != null) AddCommand(Config.BunnyHopCommand, "Bunny Hop WASD menu", _bunnyHop.OnBunnyHopCommand);
        
        AddCommand(Config.MainMenuCommand, "Main menu", OnCheatsCommand);
    }

    public void OnReloadConfigCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(player, Config.MenuPermission))
        {
            player.PrintToChat($" {ChatColors.Green}{Config.ChatTag}{ChatColors.Default} {ChatColors.Red}You do not have permission to use this command!{ChatColors.Default}");
            return;
        }

        // Eski komutları kaldır
        RemoveCommand(Config.MainMenuCommand, OnCheatsCommand);
        if (_aimbot != null) RemoveCommand(Config.AimbotCommand, _aimbot.OnAimbotCommand);
        if (_wallhack != null) RemoveCommand(Config.WallhackCommand, _wallhack.OnWallhackCommand);
        if (_magicBullet != null) RemoveCommand(Config.MagicBulletCommand, _magicBullet.OnMagicBulletCommand);
        if (_noRecoil != null) RemoveCommand(Config.NoRecoilCommand, _noRecoil.OnNoRecoilCommand);
        if (_noFlash != null) RemoveCommand(Config.NoFlashCommand, _noFlash.OnNoFlashCommand);
        if (_fragChanger != null) RemoveCommand(Config.FragChangerCommand, _fragChanger.OnFragChangerCommand);
        if (_bunnyHop != null) RemoveCommand(Config.BunnyHopCommand, _bunnyHop.OnBunnyHopCommand);

        // Config'i yeniden yükle
        Config.Reload();
        
        // Yeni komutları kaydet
        RegisterCommands();
        
        // Aktif glow renklerini güncelle
        _wallhack?.UpdateAllGlowColors();
        
        player.PrintToChat($" {ChatColors.Green}{Config.ChatTag}{ChatColors.Default} {ChatColors.Lime}Configuration and commands reloaded successfully!{ChatColors.Default}");
        Console.WriteLine($"{Config.ChatTag} Configuration reloaded by {player.PlayerName}");
    }

    public void OnCheatsCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;
        
        if (!AdminManager.PlayerHasPermissions(player, Config.MenuPermission))
        {
            player.PrintToChat($" {ChatColors.Green}{Config.ChatTag}{ChatColors.Default} {ChatColors.Red}You do not have permission to use this command!{ChatColors.Default}");
            return; 
        }

        ShowMainMenu(player);
    }

    private void ShowMainMenu(CCSPlayerController player)
    {
        WasdMenu menu = new(Config.MenuTitle + " Main Menu", this);

        menu.AddItem("🎯 Aimbot Menu", (p, o) => _aimbot?.OnAimbotCommand(p, null!));
        menu.AddItem("🧱 Wallhack Menu", (p, o) => _wallhack?.OnWallhackCommand(p, null!));
        menu.AddItem("💀 Magic Bullet Menu", (p, o) => _magicBullet?.OnMagicBulletCommand(p, null!));
        menu.AddItem("🔫 No Recoil Menu", (p, o) => _noRecoil?.OnNoRecoilCommand(p, null!));
        menu.AddItem("⚡ No Flash Menu", (p, o) => _noFlash?.OnNoFlashCommand(p, null!));
        menu.AddItem("🌟 Frag Changer Menu", (p, o) => _fragChanger?.OnFragChangerCommand(p, null!));
        menu.AddItem("🐇 Bunny Hop Menu", (p, o) => _bunnyHop?.OnBunnyHopCommand(p, null!));

        menu.Display(player, 30);
    }

    private void OnTick()
    {
        _aimbot?.OnTick();
        _wallhack?.OnTick();
        _noRecoil?.OnTick();
        _noFlash?.OnTick();
        _bunnyHop?.OnTick();
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (@event.Userid != null)
        {
            _wallhack?.OnPlayerDeath(@event.Userid);
        }

        var result1 = _magicBullet?.OnPlayerDeath(@event) ?? HookResult.Continue;
        var result2 = _fragChanger?.OnPlayerDeath(@event) ?? HookResult.Continue;

        if (result1 == HookResult.Changed || result2 == HookResult.Changed)
            return HookResult.Changed;

        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid != null)
        {
            _wallhack?.OnPlayerDisconnect(@event.Userid);
        }
        return HookResult.Continue;
    }

    private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        _magicBullet?.OnPlayerHurt(@event);
        return HookResult.Continue;
    }
}
