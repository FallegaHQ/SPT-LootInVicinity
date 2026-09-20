using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using HarmonyLib;
using Softwyx.LootInVicinity.Take;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Postfix on handler <see cref="EFT.InventoryLogic.ItemManipulator.QuickFindAppropriatePlace" />.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityQuickFindPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(ItemManipulator),
                                  nameof(ItemManipulator.QuickFindAppropriatePlace),
                                  [
                                      typeof(Item), typeof(ItemController),
                                      typeof(IEnumerable<CompoundItem>),
                                      typeof(ItemManipulator.EMoveItemOrder), typeof(bool)
                                  ]
                                 );
    }

    [PatchPostfix]
    public static void PatchPostfix(
        Item                item, ItemController controller, ItemManipulator.EMoveItemOrder order, bool simulate,
        ref OperationResult<IItemOperationResult> __result
    ){
        VicinityTakeFinalize.OnHandlerQuickFindSucceeded(item, controller, simulate, __result.Succeeded);
    }
}
