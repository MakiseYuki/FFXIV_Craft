"""
get_position.py
───────────────
Move your mouse to the FFXIV "Synthesize" button, then press F9.
The script will print and save the coordinates to 'position.txt'.

Press Escape to quit without saving.
"""

import time
import pyautogui
import keyboard

captured = {"x": None, "y": None}
done = False

def capture():
    x, y = pyautogui.position()
    captured["x"] = x
    captured["y"] = y
    global done
    done = True

def cancel():
    global done
    done = True

keyboard.add_hotkey("f9",     capture)
keyboard.add_hotkey("escape", cancel)

print("=" * 45)
print("  FFXIV Button Position Finder")
print("=" * 45)
print("  1. Switch to FFXIV")
print("  2. Hover your mouse over the Synthesize button")
print("  3. Press F9 to capture the position")
print("  Press Escape to quit")
print("=" * 45)
print()

while not done:
    x, y = pyautogui.position()
    print(f"  Current mouse position → X={x:<6} Y={y:<6}", end="\r")
    time.sleep(0.1)

print()  # newline after the live readout

if captured["x"] is not None:
    x, y = captured["x"], captured["y"]
    print(f"\n✔ Captured position: X={x}, Y={y}")
    print("\nPaste these lines into craft_macro.py:")
    print(f"  CLICK_X = {x}")
    print(f"  CLICK_Y = {y}")

    with open("position.txt", "w") as f:
        f.write(f"CLICK_X = {x}\nCLICK_Y = {y}\n")
    print("\n(Also saved to position.txt)")
else:
    print("Cancelled — no position saved.")
