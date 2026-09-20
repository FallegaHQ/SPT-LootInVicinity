using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Postfix on <see cref="ItemUiContext.QuickFindAppropriatePlace" /> (Ctrl+click).</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityItemUiQuickFindPatch : ModulePatch{
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

    [PatchPostfix]
    public static void PatchPostfix(
        ItemContext itemContext,     ItemController controller, bool forcePutInStash,
        bool                     displayWarnings, bool                  simulate,   ref ItemUiQuickFindResult __result
    ){
        VicinityTakeFinalize.OnUiQuickFindSucceeded(itemContext, controller, simulate, __result.Failed);
    }
}
