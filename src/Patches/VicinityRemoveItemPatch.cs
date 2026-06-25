using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Postfix on <see cref="GClass3017.RemoveItem" /> --
///     cleans up listed world loot after med/food consumption removes the item.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityRemoveItemPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(typeof(GClass3017), nameof(GClass3017.RemoveItem), [typeof(Item)]);
    }

    [PatchPostfix]
    public static void PatchPostfix(Item item, bool __result){
        if(!__result) return;

        VicinityListedWorldCleanup.TryCleanupAfterInventoryMutation(item, false, true);
    }
}
