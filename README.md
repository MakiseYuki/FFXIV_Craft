# CusCraft — FFXIV Crafting Loop Plugin

A [Dalamud](https://github.com/yanmucorp/Dalamud) plugin that automates a configurable crafting loop in Final Fantasy XIV.  
It double-clicks a recipe button, fires a macro key, waits for the craft to finish, and repeats — for as many cycles as you need.

---

## Requirements

- FFXIV with [yanmucorp/Dalamud](https://github.com/yanmucorp/Dalamud) (API level 12) loaded
- A craft macro already set up on your hotbar (default key: **F5**)

---

## Installation

1. Download the latest release from the [Releases](https://github.com/MakiseYuki/FFXIV_Craft/releases) page.
2. Copy the two files to your Dalamud dev-plugin folder  
   (e.g. `%AppData%\FFXIVSimpleLauncher\Dalamud\Config\devPlugins\CusCraft\`):
   - `CusCraftPlugin.dll`
   - `CusCraftPlugin.json`
3. In-game, open the Dalamud plugin installer → **Dev Tools** → load the dev plugin.

---

## Quick Start

1. Open the crafting menu and navigate to the recipe you want to mass-craft.
2. Position your mouse cursor over the **Synthesize** (confirm) button.
3. Type `/cus_craft getpos` in chat to save that screen position.
4. Type `/cus_craft config` to open the settings window and adjust the parameters.
5. Type `/cus_craft start` (or press your **Start Key** hotkey) to begin.

---

## Slash Commands

All commands are prefixed with `/cus_craft`.

| Command | Description |
|---|---|
| `/cus_craft config` | Open the settings window (also shows on `/cus_craft` alone) |
| `/cus_craft start` | Start the crafting loop (also resumes if paused) |
| `/cus_craft stop` | Stop the crafting loop immediately |
| `/cus_craft pause` | Pause or resume the loop mid-cycle |
| `/cus_craft getpos` | Save the current cursor position as CLICK_X / CLICK_Y |

---

## Settings

Open the settings window with `/cus_craft config`.  
Every field has an inline **(?)** tooltip — hover it for a detailed description.

### Craft Target

| Setting | Default | Description |
|---|---|---|
| **CLICK_X** | 2268 | Screen X coordinate of the Synthesize button. |
| **CLICK_Y** | 1498 | Screen Y coordinate of the Synthesize button. |
| **Capture current cursor position** | — | Button that saves your live mouse position as CLICK_X / CLICK_Y. Alternatively use `/cus_craft getpos`. |

> **Tip:** Move your mouse over the Synthesize button and click **Capture current cursor position** (or use `/cus_craft getpos`) to set the coordinates automatically.

### Timing

| Setting | Default | Description |
|---|---|---|
| **CRAFT_WAIT** | 10.0 s | How long to wait after pressing the macro key before starting the next cycle. Increase this if your macro runs longer. |
| **CRAFT_RECIPE_KEY** | F5 | The key sent to the game to trigger your craft macro. Change this if you have remapped the macro hotbar slot. |

### Loop

| Setting | Default | Description |
|---|---|---|
| **CRAFT_CYCLES** | 60 | Number of crafts to perform before stopping. Set to **0** for unlimited. |

### Hotkeys

Hotkeys are detected globally while the plugin is loaded and trigger on key-down (not held).  
Set a key to **— Disabled —** to turn it off.

| Hotkey | Default | Description |
|---|---|---|
| **Start Key** | Disabled | Start the crafting loop. If the loop is paused, resumes it instead. |
| **Stop Key** | Disabled | Stop the crafting loop immediately. |
| **Pause / Resume Key** | Disabled | Pause the loop mid-cycle; press again to resume. |

Available key choices: F1 – F12, Insert, Delete, Home, End, Page Up, Page Down.

---

## How the Loop Works

Each cycle performs these steps:

1. **Double-click** the saved screen coordinate (CLICK_X, CLICK_Y) to confirm the recipe.
2. Wait a brief internal delay, then press **CRAFT_RECIPE_KEY** to start the craft macro.
3. Wait **CRAFT_WAIT** seconds for the macro to finish.
4. Increment the cycle counter.  
   - Every 10 cycles a progress message is printed to chat.
5. Repeat until CRAFT_CYCLES is reached (or until stopped/paused).

The loop respects pause at every wait step, so pausing is always responsive.

---

## Troubleshooting

| Problem | Solution |
|---|---|
| Plugin fails to load with API mismatch | Ensure your Dalamud build is API level 12 (yanmucorp/Dalamud v12.x). |
| Click doesn't hit the Synthesize button | Re-capture the position with `/cus_craft getpos` or adjust CLICK_X / CLICK_Y manually. |
| Craft finishes before the next cycle starts | Increase **CRAFT_WAIT** to give the macro more time. |
| Wrong macro key fires | Change **CRAFT_RECIPE_KEY** to match the key your macro is bound to. |
| Loop runs forever | Set **CRAFT_CYCLES** to the desired count (0 = unlimited). |
