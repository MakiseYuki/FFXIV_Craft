using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace CusCraftPlugin;

public sealed class ConfigWindow : Window, IDisposable
{
    // (VK code, display label) pairs offered in the hotkey dropdowns.
    // 0 = disabled.
    private static readonly (int Vk, string Label)[] KeyChoices =
    [
        (0,    "— Disabled —"),
        (0x70, "F1"),  (0x71, "F2"),  (0x72, "F3"),  (0x73, "F4"),
        (0x74, "F5"),  (0x75, "F6"),  (0x76, "F7"),  (0x77, "F8"),
        (0x78, "F9"),  (0x79, "F10"), (0x7A, "F11"), (0x7B, "F12"),
        (0x2D, "Insert"), (0x2E, "Delete"),
        (0x24, "Home"),   (0x23, "End"),
        (0x21, "Page Up"), (0x22, "Page Down"),
    ];

    private readonly Configuration configuration;
    private readonly Action saveConfiguration;
    private readonly Func<string> getStatusText;
    private readonly Action getPosAction;

    public ConfigWindow(
        Configuration configuration,
        Action saveConfiguration,
        Func<string> getStatusText,
        Action getPosAction)
        : base("CusCraft Settings###CusCraftSettings")
    {
        this.configuration = configuration;
        this.saveConfiguration = saveConfiguration;
        this.getStatusText = getStatusText;
        this.getPosAction = getPosAction;

        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(480, 360),
            MaximumSize = new Vector2(1000, 1000),
        };
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("Slash command: /cus_craft");
        ImGui.TextWrapped("Actions: start | stop | pause | getpos | config");
        ImGui.Spacing();
        ImGui.TextColored(new Vector4(0.35f, 0.9f, 0.45f, 1.0f), $"Status: {this.getStatusText()}");
        ImGui.Separator();

        // ── Craft Target ──────────────────────────────────────────────────
        ImGui.TextDisabled("Craft Target");

        var clickX = this.configuration.ClickX;
        if (ImGui.InputInt("CLICK_X", ref clickX))
        {
            this.configuration.ClickX = clickX;
            this.saveConfiguration();
        }
        HelpMarker("Screen X coordinate of the crafting confirmation button.\nUse 'getpos' or the button below to capture it automatically.");

        var clickY = this.configuration.ClickY;
        if (ImGui.InputInt("CLICK_Y", ref clickY))
        {
            this.configuration.ClickY = clickY;
            this.saveConfiguration();
        }
        HelpMarker("Screen Y coordinate of the crafting confirmation button.\nUse 'getpos' or the button below to capture it automatically.");

        if (ImGui.Button("Capture current cursor position"))
            this.getPosAction();
        HelpMarker("Moves your mouse to the current cursor position and saves the X/Y coordinates as CLICK_X and CLICK_Y.");

        ImGui.Spacing();
        ImGui.Separator();

        // ── Timing ────────────────────────────────────────────────────────
        ImGui.TextDisabled("Timing");

        var craftWait = this.configuration.CraftWait;
        if (ImGui.InputFloat("CRAFT_WAIT (seconds)", ref craftWait, 0.1f, 1.0f, "%.1f"))
        {
            this.configuration.CraftWait = Math.Max(0.0f, craftWait);
            this.saveConfiguration();
        }
        HelpMarker("How long (in seconds) to wait after triggering the craft macro before looping.\nIncrease this if your macro takes longer to finish.");

        DrawKeyCombo("CRAFT_RECIPE_KEY", "CraftRecipeKey",
            this.configuration.CraftRecipeKey,
            v => { this.configuration.CraftRecipeKey = v; this.saveConfiguration(); },
            "The keyboard key sent to the game to start the craft macro after clicking the recipe.\nDefaults to F5 (the standard game macro key). Change this if you have remapped your macro hotbar.");

        ImGui.Spacing();
        ImGui.Separator();

        // ── Loop ──────────────────────────────────────────────────────────
        ImGui.TextDisabled("Loop");

        var craftCycles = this.configuration.CraftCycles;
        if (ImGui.InputInt("CRAFT_CYCLES", ref craftCycles))
        {
            this.configuration.CraftCycles = Math.Max(0, craftCycles);
            this.saveConfiguration();
        }
        HelpMarker("Number of crafts to perform before stopping automatically.\nSet to 0 to run indefinitely until you press the Stop hotkey.");

        ImGui.Spacing();
        ImGui.Separator();

        // ── Hotkeys ───────────────────────────────────────────────────────
        ImGui.TextDisabled("Hotkeys");
        ImGui.TextWrapped("Assign a keyboard key to each action. The key is detected globally while the plugin is loaded.");
        ImGui.Spacing();

        DrawKeyCombo("Start Key", "HotkeyStart",
            this.configuration.HotkeyStart,
            v => { this.configuration.HotkeyStart = v; this.saveConfiguration(); },
            "Press this key to start the crafting loop.\nIf the loop is paused, this key resumes it instead.");

        DrawKeyCombo("Stop Key", "HotkeyStop",
            this.configuration.HotkeyStop,
            v => { this.configuration.HotkeyStop = v; this.saveConfiguration(); },
            "Press this key to stop the crafting loop immediately.");

        DrawKeyCombo("Pause / Resume Key", "HotkeyPause",
            this.configuration.HotkeyPause,
            v => { this.configuration.HotkeyPause = v; this.saveConfiguration(); },
            "Press this key to pause the loop mid-cycle.\nPress again to resume from where it left off.");
    }

    // Renders a labelled combo box for selecting a hotkey.
    private static void DrawKeyCombo(string label, string imguiId, int currentVk, Action<int> onChanged, string tooltip)
    {
        var currentIndex = 0;
        for (var i = 0; i < KeyChoices.Length; i++)
        {
            if (KeyChoices[i].Vk == currentVk)
            {
                currentIndex = i;
                break;
            }
        }

        ImGui.SetNextItemWidth(160);
        if (ImGui.BeginCombo($"{label}###{imguiId}", KeyChoices[currentIndex].Label))
        {
            for (var i = 0; i < KeyChoices.Length; i++)
            {
                var selected = i == currentIndex;
                if (ImGui.Selectable(KeyChoices[i].Label, selected))
                    onChanged(KeyChoices[i].Vk);
                if (selected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
        HelpMarker(tooltip);
    }

    // Inline (?) tooltip helper.
    private static void HelpMarker(string text)
    {
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(text);
    }
}
