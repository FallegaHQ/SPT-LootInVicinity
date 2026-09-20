using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Postfix on <see cref="EFT.HealthSystem.MedEffectHelper.RemoveItem" /> --
///     cleans up listed world loot after med/food consumption removes the item.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityRemoveItemPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(typeof(MedEffectHelper), nameof(MedEffectHelper.RemoveItem), [typeof(Item)]);
    }

    [PatchPostfix]
    public static void PatchPostfix(Item item, bool __result){
        if(!__result) return;

        VicinityListedWorldCleanup.TryCleanupAfterInventoryMutation(item, false, true);
    }
}
