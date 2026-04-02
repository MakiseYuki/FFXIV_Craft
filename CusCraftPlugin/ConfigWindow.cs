using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace CusCraftPlugin;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly Action saveConfiguration;
    private readonly Func<string> getStatusText;
    private readonly Action startAction;
    private readonly Action stopAction;
    private readonly Action pauseAction;
    private readonly Action getPosAction;

    public ConfigWindow(
        Configuration configuration,
        Action saveConfiguration,
        Func<string> getStatusText,
        Action startAction,
        Action stopAction,
        Action pauseAction,
        Action getPosAction)
        : base("CusCraft Settings###CusCraftSettings")
    {
        this.configuration = configuration;
        this.saveConfiguration = saveConfiguration;
        this.getStatusText = getStatusText;
        this.startAction = startAction;
        this.stopAction = stopAction;
        this.pauseAction = pauseAction;
        this.getPosAction = getPosAction;

        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(430, 260),
            MaximumSize = new Vector2(1000, 1000),
        };
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("Slash command: /cus_craft");
        ImGui.TextWrapped("Supported actions: start, stop, pause, getpos, and config.");
        ImGui.Spacing();
        ImGui.TextColored(new Vector4(0.35f, 0.9f, 0.45f, 1.0f), $"Status: {this.getStatusText()}");
        ImGui.Separator();

        var clickX = this.configuration.ClickX;
        if (ImGui.InputInt("CLICK_X", ref clickX))
        {
            this.configuration.ClickX = clickX;
            this.saveConfiguration();
        }

        var clickY = this.configuration.ClickY;
        if (ImGui.InputInt("CLICK_Y", ref clickY))
        {
            this.configuration.ClickY = clickY;
            this.saveConfiguration();
        }

        var craftWait = this.configuration.CraftWait;
        if (ImGui.InputFloat("CRAFT_WAIT (seconds)", ref craftWait, 0.1f, 1.0f, "%.1f"))
        {
            this.configuration.CraftWait = Math.Max(0.0f, craftWait);
            this.saveConfiguration();
        }

        var craftCycles = this.configuration.CraftCycles;
        if (ImGui.InputInt("CRAFT_CYCLES (0 = unlimited)", ref craftCycles))
        {
            this.configuration.CraftCycles = Math.Max(0, craftCycles);
            this.saveConfiguration();
        }

        ImGui.Spacing();
        ImGui.TextDisabled("Other parameters are intentionally hidden for now, but the config structure leaves room for future versions.");
        ImGui.Spacing();

        if (ImGui.Button("Start"))
        {
            this.startAction();
        }

        ImGui.SameLine();
        if (ImGui.Button("Stop"))
        {
            this.stopAction();
        }

        ImGui.SameLine();
        if (ImGui.Button("Pause / Resume"))
        {
            this.pauseAction();
        }

        if (ImGui.Button("Use current cursor position for CLICK_X / CLICK_Y"))
        {
            this.getPosAction();
        }
    }
}
