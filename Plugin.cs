using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace NearCameraVisible;

public sealed class Plugin : IDalamudPlugin {
    private const string CommandName = "/nearcam";

    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    private readonly Configuration configuration;
    private readonly VisibilityKeeper visibilityKeeper;

    public Plugin(IDalamudPluginInterface pluginInterface) {
        PluginInterface = pluginInterface;

        configuration = Configuration.Load();
        visibilityKeeper = new VisibilityKeeper(configuration);

        ICommandManager.Get().AddHandler(CommandName, new CommandInfo(OnCommand) {
            HelpMessage = "Toggle keeping your character and mount visible when the camera is close while mounted.",
        });
    }

    private void OnCommand(string command, string arguments) {
        configuration.Enabled = !configuration.Enabled;
        configuration.Save();
        IChatGui.Get().Print($"Near Camera Visible {(configuration.Enabled ? "on" : "off")}.");
    }

    public void Dispose() {
        ICommandManager.Get().RemoveHandler(CommandName);
        visibilityKeeper.Dispose();
    }
}
