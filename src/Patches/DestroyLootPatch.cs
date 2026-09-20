using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Prefix on <see cref="GameWorld.DestroyLoot(IKillableLootItem)" /> --
///     delegates to <see cref="VicinityDestroyLootHandler" />.
/// </summary>
internal sealed class DestroyLootPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.DestroyLoot), [typeof(IKillable)]);
    }

    [PatchPrefix]
    public static bool PatchPrefix(IKillable loot){
        return VicinityDestroyLootHandler.ShouldRunVanillaDestroyLoot(loot);
    }
}
