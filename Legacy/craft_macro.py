"""
FFXIV Auto Craft Macro
======================
Sequence per craft:
  1. Click the "Synthesize" button
  2. Press Alt+7 (your in-game macro)
  3. Wait ~12 seconds for crafting to finish
  4. Repeat

Controls:
  F6  – Start / Stop the loop
  F8  – Quit the program entirely
"""

import time
import threading
import pyautogui
import keyboard

# ─────────────────────────────────────────────
#  CONFIGURATION  –  edit these values
# ─────────────────────────────────────────────

# Pixel coordinates of the "Synthesize" button.
# Run get_position.py to find the exact coordinates.
CLICK_X = 2268
CLICK_Y = 1498

# How long to wait after pressing Alt+7 before the next craft (seconds).
CRAFT_WAIT = 10.0

# Delay between clicking the button and pressing F5 (seconds).
CLICK_TO_MACRO_DELAY = 1.5

# Delay after pressing F5 to let the game register the macro input (seconds).
MACRO_START_DELAY = 0.5

# Number of crafts to run each time you start the loop.
# Set to 0 to run forever until manually stopped.
CRAFT_CYCLES = 60

# Hotkeys
HOTKEY_TOGGLE = "f6"   # start / stop the loop
HOTKEY_QUIT   = "f8"   # exit the script

# ─────────────────────────────────────────────

running  = False   # True while the craft loop is active
quitting = False   # True when the user asks to exit


def craft_loop(max_cycles=0):
    """Runs the crafting sequence repeatedly until stopped or cycle limit is reached."""
    global running, quitting

    completed = 0

    print("[INFO] Craft loop started. Press F6 to stop.")
    try:
        while running and not quitting and (max_cycles <= 0 or completed < max_cycles):
            # 1. Click the Synthesize button (twice to be sure)
            print("[STEP] Clicking Synthesize button …")
            pyautogui.click(CLICK_X, CLICK_Y)
            time.sleep(0.3)
            pyautogui.click(CLICK_X, CLICK_Y)

            time.sleep(CLICK_TO_MACRO_DELAY)

            if not running or quitting:
                break

            # 2. Press F5 to trigger the in-game macro
            print("[STEP] Pressing F5 …")
            pyautogui.press("f5")

            # Short pause to let the game register the macro input
            time.sleep(MACRO_START_DELAY)

            # 3. Wait for crafting to finish
            print(f"[WAIT] Waiting {CRAFT_WAIT}s for craft to complete …")
            for _ in range(int(CRAFT_WAIT * 10)):
                if not running or quitting:
                    break
                time.sleep(0.1)

            if not running or quitting:
                break

            completed += 1
    except pyautogui.FailSafeException:
        print("[SAFETY] PyAutoGUI fail-safe triggered (mouse at screen corner).")
        print("[SAFETY] Move mouse away from corners, then press F6 to start again.")
    finally:
        running = False
        if max_cycles > 0 and completed >= max_cycles:
            print(f"[INFO] Reached configured limit: {completed} crafts.")

        print("[INFO] Craft loop stopped.")


def toggle(event=None):
    """F6 – toggle the craft loop on/off."""
    global running
    if running:
        running = False
        print("[TOGGLE] Stopping …")
    else:
        running = True
        t = threading.Thread(target=craft_loop, args=(CRAFT_CYCLES,), daemon=True)
        t.start()
        print("[TOGGLE] Starting …")


def quit_script(event=None):
    """F8 – stop the loop and exit."""
    global running, quitting
    print("[QUIT] Exiting …")
    running  = False
    quitting = True


def main():
    print("=" * 45)
    print("  FFXIV Auto Craft Macro")
    print("=" * 45)
    print(f"  Click target : ({CLICK_X}, {CLICK_Y})")
    print(f"  Craft wait   : {CRAFT_WAIT}s")
    print(f"  Macro delay  : {MACRO_START_DELAY}s")
    if CRAFT_CYCLES > 0:
        print(f"  Max crafts   : {CRAFT_CYCLES}")
    else:
        print("  Max crafts   : unlimited")
    print(f"  Toggle       : {HOTKEY_TOGGLE.upper()}")
    print(f"  Quit         : {HOTKEY_QUIT.upper()}")
    print("=" * 45)
    print("  Edit CONFIGURATION section in craft_macro.py")
    print("  to change coordinates or timing.")
    print("=" * 45)

    keyboard.add_hotkey(HOTKEY_TOGGLE, toggle)
    keyboard.add_hotkey(HOTKEY_QUIT,   quit_script)

    print(f"\nReady! Press {HOTKEY_TOGGLE.upper()} to start crafting.\n")

    # Block until quit is requested
    while not quitting:
        time.sleep(0.1)

    print("Goodbye!")


if __name__ == "__main__":
    main()
