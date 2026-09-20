using System.Reflection;
using EFT;
using Softwyx.LootInVicinity.Config;
using Softwyx.LootInVicinity.Raid;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Postfix on <see cref="GameWorld.OnGameStarted" /> --
///     delegates to <see cref="VicinityLifecycle.OnRaidStarted" />.
/// </summary>
internal sealed class RaidStartedPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
    }

    [PatchPostfix]
    public static void PatchPostfix(){
        if(!Settings.Enabled.Value) return;

        VicinityLifecycle.OnRaidStarted();
    }
}
