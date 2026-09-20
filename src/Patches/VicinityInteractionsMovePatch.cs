using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using HarmonyLib;
using Softwyx.LootInVicinity.Take;
using Softwyx.LootInVicinity.World;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Postfix on <see cref="EFT.InventoryLogic.ItemManipulator.Move" /> --
///     delegates to <see cref="VicinityTakeFinalize.OnMoveSucceeded" />.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityInteractionsMovePatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(ItemManipulator),
                                  nameof(ItemManipulator.Move),
                                  [typeof(Item), typeof(ItemAddress), typeof(ItemController), typeof(bool)]
                                 );
    }

    [PatchPrefix]
    public static void CaptureMoveFromAddress(Item item, ref ItemAddress __state){
        __state = item?.CurrentAddress;
    }

    [PatchPostfix]
    public static void PatchPostfix(
        Item        item, ItemAddress to, ItemController itemController, bool simulate, ref OperationResult<MoveResult> __result,
        ItemAddress __state
    ){
        VicinityTakeFinalize.OnMoveSucceeded(item, to, simulate, __result.Succeeded);

        if(!VicinityListedWorldCleanup.IsListedWorldCleanupActive) return;

        VicinityListedWorldCleanup.TryCleanupAfterInventoryMutation(item, simulate, __result.Succeeded);
        VicinityListedWorldCleanup.TryCleanupAfterMoveFromAddress(__state, simulate, __result.Succeeded);
    }
}
