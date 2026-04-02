using System.Runtime.InteropServices;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace CusCraftPlugin;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "CusCraft";

    private const string CommandName = "/cus_craft";

    private readonly IDalamudPluginInterface pluginInterface;
    private readonly ICommandManager commandManager;
    private readonly IChatGui chatGui;
    private readonly WindowSystem windowSystem = new("CusCraft");
    private readonly Configuration configuration;
    private readonly ConfigWindow configWindow;
    private readonly object stateLock = new();

    private CancellationTokenSource? loopCts;
    private Task? loopTask;
    private volatile bool paused;
    private bool disposed;
    private int completedCycles;

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IChatGui chatGui)
    {
        this.pluginInterface = pluginInterface;
        this.commandManager = commandManager;
        this.chatGui = chatGui;

        this.configuration = this.pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.configuration.Initialize(this.pluginInterface);

        this.configWindow = new ConfigWindow(
            this.configuration,
            this.SaveConfiguration,
            this.GetStatusText,
            this.StartCrafting,
            () => this.StopCrafting(),
            this.TogglePause,
            this.CaptureCursorPosition);

        this.windowSystem.AddWindow(this.configWindow);

        this.commandManager.AddHandler(CommandName, new CommandInfo(this.OnCommand)
        {
            HelpMessage = "/cus_craft start | stop | pause | getpos | config",
            ShowInHelp = true,
        });

        this.pluginInterface.UiBuilder.Draw += this.DrawUi;
        this.pluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUi;

        this.chatGui.Print("CusCraft loaded. Use /cus_craft config to open settings.");
    }

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        this.StopCrafting(printMessage: false);

        this.commandManager.RemoveHandler(CommandName);
        this.pluginInterface.UiBuilder.Draw -= this.DrawUi;
        this.pluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfigUi;

        this.windowSystem.RemoveAllWindows();
        this.configWindow.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        var action = (args ?? string.Empty).Trim().ToLowerInvariant();

        switch (action)
        {
            case "":
            case "config":
            case "settings":
                this.OpenConfigUi();
                this.PrintUsage();
                break;

            case "start":
                this.StartCrafting();
                break;

            case "stop":
                this.StopCrafting();
                break;

            case "pause":
                this.TogglePause();
                break;

            case "getpos":
                this.CaptureCursorPosition();
                break;

            default:
                this.chatGui.PrintError($"Unknown CusCraft action: '{args}'.");
                this.PrintUsage();
                break;
        }
    }

    private void StartCrafting()
    {
        lock (this.stateLock)
        {
            if (this.loopTask is { IsCompleted: false })
            {
                if (this.paused)
                {
                    this.paused = false;
                    this.chatGui.Print("CusCraft resumed.");
                }
                else
                {
                    this.chatGui.Print("CusCraft is already running.");
                }

                return;
            }

            this.completedCycles = 0;
            this.paused = false;

            var cts = new CancellationTokenSource();
            this.loopCts = cts;
            this.loopTask = Task.Run(() => this.RunLoopAsync(cts));
        }

        var cycleText = this.configuration.CraftCycles <= 0
            ? "unlimited"
            : this.configuration.CraftCycles.ToString();

        this.chatGui.Print(
            $"CusCraft started. Target=({this.configuration.ClickX}, {this.configuration.ClickY}), wait={this.configuration.CraftWait:0.0}s, cycles={cycleText}.");
    }

    private void StopCrafting(bool printMessage = true)
    {
        CancellationTokenSource? ctsToCancel;

        lock (this.stateLock)
        {
            if (this.loopTask is not { IsCompleted: false } || this.loopCts is null)
            {
                if (printMessage && !this.disposed)
                {
                    this.chatGui.Print("CusCraft is not running.");
                }

                return;
            }

            ctsToCancel = this.loopCts;
            this.paused = false;
        }

        ctsToCancel.Cancel();

        if (printMessage && !this.disposed)
        {
            this.chatGui.Print("CusCraft stopped.");
        }
    }

    private void TogglePause()
    {
        lock (this.stateLock)
        {
            if (this.loopTask is not { IsCompleted: false })
            {
                this.chatGui.Print("CusCraft is not running.");
                return;
            }

            this.paused = !this.paused;
        }

        this.chatGui.Print(this.paused ? "CusCraft paused." : "CusCraft resumed.");
    }

    private void CaptureCursorPosition()
    {
        if (!NativeMethods.TryGetCursorPosition(out var point))
        {
            this.chatGui.PrintError("CusCraft could not read the current cursor position.");
            return;
        }

        this.configuration.ClickX = point.X;
        this.configuration.ClickY = point.Y;
        this.SaveConfiguration();

        this.chatGui.Print($"CusCraft position updated: CLICK_X={point.X}, CLICK_Y={point.Y}");
    }

    private async Task RunLoopAsync(CancellationTokenSource cts)
    {
        var token = cts.Token;

        try
        {
            while (!token.IsCancellationRequested)
            {
                await this.WaitWhilePausedAsync(token).ConfigureAwait(false);

                if (this.configuration.CraftCycles > 0 && this.completedCycles >= this.configuration.CraftCycles)
                {
                    if (!this.disposed)
                    {
                        this.chatGui.Print($"CusCraft completed {this.completedCycles} craft(s) and stopped.");
                    }

                    return;
                }

                NativeMethods.DoubleLeftClickAt(this.configuration.ClickX, this.configuration.ClickY);
                await this.WaitResponsiveAsync(this.configuration.ClickToMacroDelay, token).ConfigureAwait(false);

                NativeMethods.PressVirtualKey(NativeMethods.VirtualKeyF5);
                await this.WaitResponsiveAsync(this.configuration.MacroStartDelay, token).ConfigureAwait(false);
                await this.WaitResponsiveAsync(this.configuration.CraftWait, token).ConfigureAwait(false);

                var currentCount = Interlocked.Increment(ref this.completedCycles);
                if (!this.disposed && (currentCount == 1 || currentCount % 10 == 0))
                {
                    this.chatGui.Print($"CusCraft progress: {currentCount} craft(s) completed.");
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            if (!this.disposed)
            {
                this.chatGui.PrintError($"CusCraft error: {ex.Message}");
            }
        }
        finally
        {
            lock (this.stateLock)
            {
                if (ReferenceEquals(this.loopCts, cts))
                {
                    this.loopCts = null;
                    this.loopTask = null;
                }

                this.paused = false;
            }

            cts.Dispose();
        }
    }

    private async Task WaitWhilePausedAsync(CancellationToken token)
    {
        while (this.paused && !token.IsCancellationRequested)
        {
            await Task.Delay(150, token).ConfigureAwait(false);
        }
    }

    private async Task WaitResponsiveAsync(double seconds, CancellationToken token)
    {
        var remaining = Math.Max(0.0, seconds);

        while (remaining > 0.0)
        {
            await this.WaitWhilePausedAsync(token).ConfigureAwait(false);

            var slice = Math.Min(remaining, 0.1);
            await Task.Delay(TimeSpan.FromSeconds(slice), token).ConfigureAwait(false);
            remaining -= slice;
        }
    }

    private string GetStatusText()
    {
        var running = this.loopTask is { IsCompleted: false };
        if (!running)
        {
            return "Stopped";
        }

        return this.paused
            ? $"Paused ({this.completedCycles} completed)"
            : $"Running ({this.completedCycles} completed)";
    }

    private void SaveConfiguration()
    {
        this.configuration.Save();
    }

    private void OpenConfigUi()
    {
        this.configWindow.IsOpen = true;
    }

    private void DrawUi()
    {
        this.windowSystem.Draw();
    }

    private void PrintUsage()
    {
        this.chatGui.Print("/cus_craft start | stop | pause | getpos | config");
    }
}

internal static class NativeMethods
{
    internal const byte VirtualKeyF5 = 0x74;

    private const uint MouseEventLeftDown = 0x0002;
    private const uint MouseEventLeftUp = 0x0004;
    private const uint KeyEventKeyUp = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Point
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point point);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, nuint extraInfo);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, nuint extraInfo);

    internal static bool TryGetCursorPosition(out Point point)
    {
        return GetCursorPos(out point);
    }

    internal static void DoubleLeftClickAt(int x, int y)
    {
        _ = GetCursorPos(out var originalPosition);

        SetCursorPos(x, y);
        LeftClick();
        Thread.Sleep(75);
        LeftClick();
        Thread.Sleep(50);

        SetCursorPos(originalPosition.X, originalPosition.Y);
    }

    internal static void PressVirtualKey(byte virtualKey)
    {
        keybd_event(virtualKey, 0, 0, 0);
        Thread.Sleep(30);
        keybd_event(virtualKey, 0, KeyEventKeyUp, 0);
    }

    private static void LeftClick()
    {
        mouse_event(MouseEventLeftDown, 0, 0, 0, 0);
        Thread.Sleep(20);
        mouse_event(MouseEventLeftUp, 0, 0, 0, 0);
    }
}
