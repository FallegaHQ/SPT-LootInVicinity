using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Softwyx.LootInVicinity;

internal static class PluginPaths{
    private const string AssetsFolderName  = "assets";
    private const string LocalesFolderName = "locales";

    private static string PluginDirectory =>
        Path.GetDirectoryName(LootInVicinityPlugin.Instance?.Info.Location) ?? string.Empty;

    private static string AssetsDirectory  => Path.Combine(PluginDirectory, AssetsFolderName);
    public static  string LocalesDirectory => Path.Combine(PluginDirectory, LocalesFolderName);

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static string AssetFile(string fileName){
        return Path.Combine(AssetsDirectory, fileName);
    }
}
