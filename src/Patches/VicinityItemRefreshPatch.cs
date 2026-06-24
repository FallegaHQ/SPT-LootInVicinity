using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Postfix on <see cref="Item.RaiseRefreshEvent" /> --
///     cleans up depleted listed world loot after use or unload.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityItemRefreshPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(typeof(Item), nameof(Item.RaiseRefreshEvent), [typeof(bool), typeof(bool)]);
    }

    [PatchPostfix]
    public static void PatchPostfix(Item __instance){
        VicinityListedWorldCleanup.TryCleanupOnItemRefresh(__instance);
    }
}
