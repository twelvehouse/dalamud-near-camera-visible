using Dalamud.Configuration;

namespace NearCameraVisible;

public sealed class Configuration : IPluginConfiguration {
    public int Version { get; set; } = 1;

    public bool Enabled { get; set; } = true;

    public static Configuration Load()
        => Plugin.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

    public void Save()
        => Plugin.PluginInterface.SavePluginConfig(this);
}
