using System.Diagnostics.CodeAnalysis;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Postfix on <see cref="ItemUiContext.QuickFindAppropriatePlace"/> (Ctrl+click).</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityItemUiQuickFindPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(ItemUiContext),
                                  nameof(ItemUiContext.QuickFindAppropriatePlace),
                                  [
                                      typeof(ItemContextAbstractClass), typeof(TraderControllerClass), typeof(bool),
                                      typeof(bool), typeof(bool)
                                  ]
                                 );
    }

    [PatchPostfix]
    public static void PatchPostfix(
        ItemContextAbstractClass itemContext,     TraderControllerClass controller, bool forcePutInStash,
        bool                     displayWarnings, bool                  simulate,   ref ItemUiQuickFindResult __result
    ){
        VicinityTakeFinalize.OnUiQuickFindSucceeded(itemContext, controller, simulate, __result.Failed);
    }
}
