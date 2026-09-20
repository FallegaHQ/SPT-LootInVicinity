using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using Softwyx.LootInVicinity.World;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Postfix on <see cref="Item.RaiseRefreshEvent" /> --
///     cleans up depleted listed world loot after use or unload.
///
///     This class is currently unused and is kept for reference only. Might remove later.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class VicinityItemRefreshPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(typeof(Item), nameof(Item.RaiseRefreshEvent), [typeof(bool), typeof(bool)]);
    }

    [PatchPostfix]
    public static void PatchPostfix(Item __instance){
        VicinityListedWorldCleanup.TryCleanupOnItemRefresh(__instance);
    }
}
