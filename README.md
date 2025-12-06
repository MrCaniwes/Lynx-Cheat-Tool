#  Lynx Cheat Tool

![CS2](https://img.shields.io/badge/Game-CS2-orange?style=for-the-badge&logo=counter-strike)
![Platform](https://img.shields.io/badge/Platform-CounterStrikeSharp-blue?style=for-the-badge)
![Runtime](https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**Lynx Cheat Tool** is a premium, highly configurable, and modular cheat plugin developed for **Counter-Strike 2** servers. Built on top of the robust **CounterStrikeSharp** framework, it provides server administrators with a suite of advanced features for testing, trolling, or simply having fun.


---

## 🌟 Features

### 🎯 Aimbot
*   **Automatic Lock:** Instantly locks onto enemy heads with precision.
*   **Sticky Targeting:** Once locked, the crosshair stays glued to the target (360° FOV support).
*   **Smart Targeting:** Automatically ignores teammates and dead players.
*   **Smooth Aim:** Configurable smoothing for more "legit" or "rage" looking aim.

### 🧱 Wallhack (ESP)
*   **Glow Effect:** Highlights enemies through walls with a customizable color.
*   **Team Awareness:** Only highlights enemies, keeping your screen clean.
*   **Dynamic Rendering:** Uses a double-entity system (`prop_dynamic`) for flicker-free visuals.

### 🐇 Bunny Hop
*   **Auto Jump:** Just hold `SPACE` to jump continuously.
*   **Velocity Maintenance:** Perfect timing for maintaining maximum speed.

### 💀 Magic Bullet (One Shot)
*   **Instant Kill:** Any hit on an enemy (leg, arm, body) results in instant death.
*   **Headshot Force:** Forces the kill to be registered as a **Headshot** 🎯 in the killfeed.
*   **Silent Death:** Victims drop instantly without knowing what hit them.

### 🔫 No Recoil
*   **Laser Accuracy:** Completely removes weapon recoil and visual punch.
*   **Server-Side:** Works independently of client-side settings.
*   **Perfect Spray:** All bullets hit exactly where the crosshair is aimed.

### ⚡ No Flash
*   **Total Immunity:** Flashbangs have absolutely no effect.
*   **Instant Recovery:** Removes existing flash effects instantly upon activation.
*   **Sunglasses Mode:** 😎 Play without ever being blinded.

### 🌟 Frag Changer (Killfeed Customizer)
Customize your killfeed presence with unique icons. When you get a kill, you can force **ALL** or **SELECTED** icons to appear:
*   🎯 **Headshot**
*   🧱 **Penetrated** (Wallbang)
*   ☁️ **Thru Smoke**
*   🕶️ **Blind**
*   🚫 **Noscope**
*   🦅 **AttackerAir** (Airborne)
*   ☠️ **Domination / Revenge / Wipe**

---

## ⚙️ Configuration & Localization

Lynx Cheat Tool is designed to be fully customizable.

### 📄 Configuration (`LynxCheatTool.json`)
*   **Permissions:** Set custom permissions for *each* feature (e.g., `@css/root`, `@css/admin`).
*   **Commands:** Rename any command to your liking (e.g., change `!aimbot` to `!aim`).
*   **Settings:** Tweak Aimbot FOV, Smoothness, Wallhack Colors (RGB), and more.
---

## 🎮 Commands

All commands are configurable in `LynxCheatTool.json`. Default commands are:

| Command | Description | Permission (Default) |
| :--- | :--- | :--- |
| `!cheats` / `css_cheats` | Opens the **Main Menu** (Hub for all features). | `@css/root` |
| `!aimbot` | Opens the **Aimbot** menu. | `@css/root` |
| `!wallhack` | Opens the **Wallhack** menu. | `@css/root` |
| `!bunnyhop` | Opens the **Bunny Hop** menu. | `@css/root` |
| `!magicbullet` | Opens the **Magic Bullet** menu. | `@css/root` |
| `!fragchanger` | Opens the **Frag Changer** menu. | `@css/root` |
| `!norecoil` | Opens the **No Recoil** menu. | `@css/root` |
| `!noflash` | Opens the **No Flash** menu. | `@css/root` |
| `!lynx_reload` | Reloads config file without restarting. | `@css/root` |

---

## 📋 Menu Usage (WASD)

The plugin utilizes the intuitive **WASD Menu** system:

*   **W / S:** Navigate Up / Down
*   **A / D:** Enter Menu / Go Back
*   **E:** Confirm Selection / Toggle Feature

### 👥 "Toggle All" System
Every menu features a **"👥 Toggle All"** option. This powerful tool allows admins to instantly enable or disable a specific cheat for **everyone** on the server. Perfect for chaos events!

---

---

## 🗺️ Roadmap

- [x] Codes have been organized
- [ ] Full language support will be added
- [ ] New cheats will be added

---

## 📦 Installation

1.  **Prerequisites:**
    *   [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) (Latest)
    *   [CS2MenuManager](https://github.com/schwarper/CS2MenuManager)
    *   .NET 8.0 Runtime

2.  **Setup:**
    *   Compile the project or download the release.
    *   Place `LynxCheatTool.dll` (and dependencies) into `addons/counterstrikesharp/plugins/LynxCheatTool/`.
    *   Restart the server.
    *   Configs will be auto-generated in `addons/counterstrikesharp/configs/plugins/LynxCheatTool/`.

---

## ⚠️ Disclaimer

This plugin is intended for **EDUCATIONAL and ENTERTAINMENT** purposes only.

---

Developed with ❤️ by **LynxHera**.

*   **Wallhack Method:** ESP-Players-GoldKingZ
*   **Menu System:** CS2MenuManager by Schwarper
