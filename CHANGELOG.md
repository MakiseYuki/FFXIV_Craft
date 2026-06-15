# Changelog

All notable changes to CusCraft will be documented in this file.

---

## [1.1.0] - 2026-06-16

### Added
- **Advanced Crafting (two-macro mode)**: a new checkbox in settings enables a second macro key for crafting rotations that are too long to fit in a single in-game macro.
  - **CRAFT_RECIPE_KEY_2** — the key that triggers the second macro (default F6).
  - **MACRO_BETWEEN_WAIT** — configurable wait time (in seconds) between the end of Macro 1 and the start of Macro 2 (default 3.0 s).
- Updated README with full documentation for the new feature, timing diagram, and troubleshooting entries.

### Notes
- `CRAFT_WAIT` continues to apply per-macro. In advanced mode the total per-craft wait is `(CRAFT_WAIT × 2) + MACRO_BETWEEN_WAIT`.
- When Advanced Crafting is **disabled** the plugin behaves exactly as before — no behaviour change for existing users.

---

## [1.0.1] - Prior release

- Hotkey edge-detection: start, stop, and pause/resume via global keyboard shortcuts.
- Hidden `ClickToMacroDelay` and `MacroStartDelay` internal parameters for fine-tuning timing.

## [1.0.0] - Initial release

- Core crafting loop: click Synthesize, press macro key, wait, repeat.
- Configurable click coordinates, craft key, wait time, and cycle count.
- Chat commands: start, stop, pause, getpos, config.
- ImGui settings window with tooltips.
