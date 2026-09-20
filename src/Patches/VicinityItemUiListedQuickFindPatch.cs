using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Prefix on <see cref="ItemUiContext.QuickFindAppropriatePlace" /> --
///     listed world rows quick-move to player equipment.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityItemUiListedQuickFindPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(ItemUiContext),
                                  nameof(ItemUiContext.QuickFindAppropriatePlace),
                                  [
                                      typeof(ItemContext), typeof(ItemController), typeof(bool),
                                      typeof(bool), typeof(bool)
                                  ]
                                 );
    }

    [PatchPrefix]
    public static bool PatchPrefix(
        ItemContext itemContext,     ItemController controller, bool forcePutInStash,
        bool                     displayWarnings, bool                  simulate,   ref ItemUiQuickFindResult __result
    ){
        return VicinityListedQuickFindHandler.TryQuickFindListedWorldItemToPlayer(
             itemContext,
             controller,
             forcePutInStash,
             displayWarnings,
             simulate,
             out __result
            );
    }
}
