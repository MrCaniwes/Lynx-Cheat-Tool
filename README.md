# 🛡️ Lynx Cheat Tool (Modular Edition)

![Game](https://img.shields.io/badge/Game-CS2-orange?style=for-the-badge&logo=counter-strike)
![Platform](https://img.shields.io/badge/Platform-CounterStrikeSharp-blue?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Optimized-green?style=for-the-badge)

**Lynx Cheat Tool** is a high-performance, modular, and fully configurable administrative cheat suite for **Counter-Strike 2**, built on the CounterStrikeSharp framework.

> [!IMPORTANT]
> **Evolution of the Tool:** This version represents a complete architectural overhaul. **Targeting Command System**.

---

## 🚀 Key Improvements
*   **Modular Architecture:** Each feature (ESP, Aimbot, OneTap, etc.) is a standalone module. This prevents global crashes and allows for easy updates.
*   **Smart Targeting System:** No more scrolling through menus. Control anyone instantly using `@all`, `@ct`, `@t`, `#userid`, or partial player names.
*   **Performance Optimization:** Optimized entity management and listener logic. We have eliminated "SteamNetworkingSockets lock held" warnings, ensuring a smooth experience even on high-pop servers.
*   **Deep Localization:** Fully localized via JSON files. Customize every message, color, and status report in `tr.json` or `en.json`.

---

## 🌟 Core Modules

### 🎯 Aimbot (Fully Dynamic)
*   **Precision calculation:** Advanced mathematical angle normalization.
*   **Live Tuning:** Commands like `!aimfov`, `!aimsmooth`, and `!aimsticky` allow real-time adjustments without restarts.
*   **Team Filter:** Automatically ignores teammates.

### 🧱 Professional ESP (Glow)
*   **Engine-Level Glow:** Uses the Counter-Strike 2 internal glow system via optimized entity pooling.
*   **CheckTransmit Integration:** Only authorized players see the ESP; it remains invisible to regular players.
*   **Lag-Free Spawning:** Fixed the entity-burst issue during round starts.

### 💀 One-Tap (Magic Bullet)
*   **Perfect Kill:** Any hit on a target is registered as a fatal blow.

### 🐇 Bunny Hop
*   **Z-Axis Manipulation:** Smooth, continuous jumping that maintains and boosts velocity for perfect movement.

### 👓 No-Flash
*   **Total Clarity:** Instantly removes flashbang blindness effects.

### 📊 Live Status System
*   **Real-time Audit:** Use `!status <target>` to see an instant report of all active cheats on any player.

---

## 🎯 Smart Targeting Syntax
Our All commands allows for surgical precision:

| Target | Description | Example |
| :--- | :--- | :--- |
| `@all` | Every player on the server | `!wh @all` |
| `@ct` / `@t` | Apply to a specific team | `!ot @t` |
| `#ID` | Target by Slot or UserID | `!bhop #12` |
| `Name` | Partial nickname match | `!inv lynx` |
| `Empty` | Targets yourself | `!aim` |

---

## ⚙️ Configuration & Customization
Edit `LynxCheatTool.json` to take full control:
*   **Rename Commands:** Map any feature to your preferred command name (e.g., `!aim` instead of `!aimbot`).
*   **Permission Control:** Gate features behind specific SteamIDs, Admin Flags (`@css/root`), or Groups.
*   **Dynamic Defaults:** Set your own FOV, Colors, and Smoothness constants.

---

## 📦 Installation
1.  Ensure [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) is installed.
2.  Deploy `LynxCheatTool.dll` and the `lang` directory to your plugins folder.
3.  Configure `lang/tr.json` or `lang/en.json` for your community.
4.  Restart the server and dominate.

---

## 🗺️ Roadmap
- [x] Complete Architectural Overhaul (Modular System)
- [x] Optimized Performance (Lock Held issues fixed)
- [x] Smart Targeting System (@all, @ct, @t, #ID)
- [x] Multi-language Localization (TR/EN Support) 🌍
- [ ] New Advanced Cheats (Triggerbot, Infinite Ammo, etc.)


---

Developed with ❤️ by **MrCaniwes**

> [!NOTE]
> **🤖 Fully Architected by AI:** This entire project, including its modular framework and performance optimizations, was developed and refined by **AI**. Future-proof modding at its finest.

 **Credits:**
*   **ESP Module:** Based on [cs2-ESP-Players-GoldKingZ](https://github.com/oqyh/cs2-ESP-Players-GoldKingZ) by oqyh.
