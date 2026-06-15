# CusCraft — FFXIV Crafting Loop Plugin

CusCraft is a plugin for Final Fantasy XIV that automates repeating a craft over and over so you don't have to click the same button hundreds of times.  
You tell it **where** the Synthesize button is on your screen, **which key** fires your craft macro, and **how many** items you want — then just let it run.

---

## Before You Begin

You need two things installed before CusCraft will work:

- **Final Fantasy XIV** (PC version)
- **Dalamud** — a free plugin framework for FFXIV. Get it via [XIVTCLauncher](https://github.com/cycleapple/XIVTCLauncher), which installs Dalamud automatically.

You also need a **craft macro** already set up on your hotbar (the plugin sends a keypress to trigger it — F5 by default).  
If you don't have a macro yet, search "FFXIV crafting macro" for guides on the official forums or Reddit.

---

## Installation

1. Go to the [Releases](https://github.com/MakiseYuki/FFXIV_Craft/releases) page and download the latest release.
2. Extract and copy these two files into your Dalamud dev-plugin folder:
   - `CusCraftPlugin.dll`
   - `CusCraftPlugin.json`

   The folder is usually at:
   ```
   %AppData%\FFXIVSimpleLauncher\Dalamud\Config\devPlugins\CusCraft\
   ```
   (Create the `CusCraft` folder if it doesn't exist.)

3. Launch FFXIV through XIVTCLauncher, then open the Dalamud Plugin Installer (`/xlplugins` in chat) → **Dev Tools** tab → enable CusCraft.

---

## Setup

Before starting a crafting loop, you need to tell the plugin exactly where the **Synthesize** button is on *your* screen:

1. Open the crafting menu in-game and hover your mouse directly over the **Synthesize** button.
2. Type `/cus_craft getpos` in the chat box and press Enter.  
   The plugin saves that position — you're done.

> **Why is this needed?** The plugin clicks that button automatically for you. Every monitor and UI layout is different, so it needs your coordinates once.
> **Note:** If you change your UI layout or switch to a different monitor, you'll need to repeat this step.

---

## Quick Start

1. Open the crafting menu and navigate to the recipe you want to mass-craft.
2. Make sure you've done the **Setup** if your UI layout has changed since last time.
3. Type `/cus_craft start` in chat — or press your configured **Start Key** hotkey.
4. The plugin will begin crafting automatically. To stop early, type `/cus_craft stop`.

---

## Chat Commands

Type these in the FFXIV chat box:

| Command | What it does |
|---|---|
| `/cus_craft config` | Open the settings window |
| `/cus_craft start` | Start crafting (also resumes if paused) |
| `/cus_craft stop` | Stop immediately |
| `/cus_craft pause` | Pause mid-loop; type again to resume |
| `/cus_craft getpos` | Save your current mouse position as the click target |

---

## Settings

Open the settings window any time with `/cus_craft config` or from the Plugin Installer.  
Hover any **(?)** icon in the window for a detailed description of that setting.

### Where to click

| Setting | Default | Description |
|---|---|---|
| **CLICK_X / CLICK_Y** | 2268 / 1498 | Screen coordinates of the Synthesize button. Use the **Capture** button or `/cus_craft getpos` to set these automatically. |

> **Note:** When using the Capture button, make sure to hover your mouse directly over the Synthesize button before clicking Capture.

### Timing

| Setting | Default | Description |
|---|---|---|
| **CRAFT_WAIT** | 10.0 s | How long to wait after your macro starts before the next craft begins. If the plugin starts the next craft too early, increase this to match your macro's length. |
| **CRAFT_RECIPE_KEY** | F5 | The key that triggers your craft macro. Change this if your macro is on a different hotbar slot. |

### Advanced Crafting (two-macro mode)

Some crafting rotations are too long to fit in a single in-game macro (max 15 lines per macro). FFXIV players typically split these into two consecutive macros. Enable **Advanced Crafting** to support this.

When active, the loop becomes:

```
[Click Synthesize] → wait → [Press Macro 1 Key] → CRAFT_WAIT
                          → MACRO_BETWEEN_WAIT
                          → [Press Macro 2 Key] → CRAFT_WAIT
→ (next craft cycle)
```

The **total time spent per craft** in advanced mode is:
> `(CRAFT_WAIT × 2) + MACRO_BETWEEN_WAIT`  *(plus small internal delays)*

| Setting | Default | Description |
|---|---|---|
| **Enable Advanced Crafting** | Off | Toggle to activate two-macro mode. When off, only Macro 1 is used (original behaviour). |
| **CRAFT_RECIPE_KEY_2** | F6 | The key that triggers your **second** craft macro. |
| **MACRO_BETWEEN_WAIT** | 3.0 s | How long to wait after Macro 1 finishes before pressing the Macro 2 key. Increase this if you see the second macro being missed. |

> **Tip:** `CRAFT_WAIT` is a per-macro wait. Set it to however long **one** of your macros takes to complete. Both macros share this value.

### Number of crafts

| Setting | Default | Description |
|---|---|---|
| **CRAFT_CYCLES** | 60 | How many times to craft before stopping. Set to **0** to run forever until you stop it manually. |

### Hotkeys (optional)

You can assign keyboard shortcuts to start, stop, and pause the loop without typing in chat.  
Set any hotkey to **— Disabled —** to leave it unbound.

> **Note:** If hotkey is disabled, you can still control the plugin with chat commands. Hotkeys are just a convenient alternative.

| Hotkey | Default | Description |
|---|---|---|
| **Start Key** | Disabled | Start (or resume) the crafting loop. |
| **Stop Key** | Disabled | Stop the loop immediately. |
| **Pause / Resume Key** | Disabled | Pause mid-cycle; press again to resume. |

Available keys: F1 – F12, Insert, Delete, Home, End, Page Up, Page Down.

---

## Common Troubleshooting

| Problem | Solution |
|---|---|
| The plugin doesn't appear in the installer | Make sure both `CusCraftPlugin.dll` and `CusCraftPlugin.json` are in the correct folder and that you're in the **Dev Tools** tab. |
| The click misses the Synthesize button | Redo the **Setup**: hover your mouse over the button and type `/cus_craft getpos`. |
| The next craft starts before the current one finishes | Increase **CRAFT_WAIT** in settings to match your macro's total duration. |
| The wrong macro fires | Change **CRAFT_RECIPE_KEY** to whatever key your craft macro is bound to. |
| It keeps crafting and won't stop on its own | Set **CRAFT_CYCLES** to the number of items you want (0 means unlimited). |
| Plugin fails to load | Make sure XIVTCLauncher and Dalamud are up to date. |
| Macro 2 never fires in advanced mode | Make sure **Enable Advanced Crafting** is checked in settings and **CRAFT_RECIPE_KEY_2** is set to the correct key. |
| Macro 2 fires too early / craft is incomplete | Increase **MACRO_BETWEEN_WAIT** to give Macro 1 more time to finish before Macro 2 is triggered. |
| The craft finishes but the next cycle starts too soon | In advanced mode, `CRAFT_WAIT` applies to **each** macro. Increase it to match the longer of your two macros. |
