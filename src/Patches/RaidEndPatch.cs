using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Postfix on raid end --
/// always delegates to <see cref="VicinityLifecycle.OnRaidEnded"/>.</summary>
internal sealed class RaidEndPatch : ModulePatch{
    private static readonly string[] RaidEndMethodNames = ["OnGameSessionEnd", "OnGameEnded", "Dispose"];

    private const BindingFlags GameWorldMethodFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    protected override MethodBase GetTargetMethod(){
        foreach(var name in RaidEndMethodNames){
            var method = typeof(GameWorld).GetMethod(name, GameWorldMethodFlags);

            if(method != null) return method;
        }

        LootInVicinityPlugin.Log?.LogWarning(PluginInfo.Format("RaidEndPatch: no GameWorld end method found."));

        return null;
    }

    [PatchPostfix]
    public static void PatchPostfix(){
        VicinityLifecycle.OnRaidEnded();
    }
}
