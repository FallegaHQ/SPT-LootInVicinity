using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Postfix on handler <see cref="InteractionsHandlerClass.QuickFindAppropriatePlace" />.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityQuickFindPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(InteractionsHandlerClass),
                                  nameof(InteractionsHandlerClass.QuickFindAppropriatePlace),
                                  [
                                      typeof(Item), typeof(TraderControllerClass),
                                      typeof(IEnumerable<CompoundItem>),
                                      typeof(InteractionsHandlerClass.EMoveItemOrder), typeof(bool)
                                  ]
                                 );
    }

    [PatchPostfix]
    public static void PatchPostfix(
        Item item, TraderControllerClass controller, InteractionsHandlerClass.EMoveItemOrder order, bool simulate,
        ref QuickFindResult __result
    ){
        VicinityTakeFinalize.OnHandlerQuickFindSucceeded(item, controller, simulate, __result.Succeeded);
    }
}
