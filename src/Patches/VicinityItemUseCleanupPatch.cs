using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Postfix on <see cref="ItemUiContext" /> use actions --
///     deferred cleanup for depleted listed world loot (avoids re-entrancy during med effects).
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityItemUseCleanupPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(typeof(ItemUiContext), nameof(ItemUiContext.UseItem), [typeof(Item)]);
    }

    [PatchPostfix]
    public static void UseItemPostfix(Item item){
        VicinityListedWorldCleanup.ScheduleObsoleteListedWorldCleanup(item);
    }
}

/// <summary>Postfix on <see cref="ItemUiContext.UseAll" />.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityItemUseAllCleanupPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(typeof(ItemUiContext), nameof(ItemUiContext.UseAll), [typeof(Item)]);
    }

    [PatchPostfix]
    public static void UseAllPostfix(Item item){
        VicinityListedWorldCleanup.ScheduleObsoleteListedWorldCleanup(item);
    }
}
