using System.Diagnostics.CodeAnalysis;

namespace Softwyx.LootInVicinity;

/// <summary>Plugin identity. <see cref="PLUGIN_VERSION" /> is generated in PluginInfo.Version.g.cs on each build.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static partial class PluginInfo{
    public const  string PLUGIN_GUID = "com.softwyx.lootinvicinity";
    public const  string PLUGIN_NAME = "Loot In Vicinity";
    private const string LogPrefix   = "LIV";

    public static string Format(string message){
        return $"[{LogPrefix}] {message}";
    }
}
