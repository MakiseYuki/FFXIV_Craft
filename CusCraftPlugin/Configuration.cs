using Dalamud.Configuration;
using Dalamud.Plugin;

namespace CusCraftPlugin;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public int ClickX { get; set; } = 2268;
    public int ClickY { get; set; } = 1498;
    public float CraftWait { get; set; } = 10.0f;
    public int CraftCycles { get; set; } = 60;

    // 0 = disabled; otherwise a Windows VirtualKey code (e.g. VK_F5 = 0x74).
    public int HotkeyStart { get; set; } = 0;
    public int HotkeyStop { get; set; } = 0;
    public int HotkeyPause { get; set; } = 0;

    // Hidden for now, but kept in config for future expansion.
    public float ClickToMacroDelay { get; set; } = 1.5f;
    public float MacroStartDelay { get; set; } = 0.5f;

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        this.pluginInterface?.SavePluginConfig(this);
    }
}
