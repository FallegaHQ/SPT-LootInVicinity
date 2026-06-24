using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Postfix on <see cref="InteractionsHandlerClass.Move" /> --
///     delegates to <see cref="VicinityTakeFinalize.OnMoveSucceeded" />.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityInteractionsMovePatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(InteractionsHandlerClass),
                                  nameof(InteractionsHandlerClass.Move),
                                  [typeof(Item), typeof(ItemAddress), typeof(TraderControllerClass), typeof(bool)]
                                 );
    }

    [PatchPrefix]
    public static void CaptureMoveFromAddress(Item item, ref ItemAddress __state){
        __state = item?.CurrentAddress;
    }

    [PatchPostfix]
    public static void PatchPostfix(
        Item        item, ItemAddress to, TraderControllerClass itemController, bool simulate, ref MoveResult __result,
        ItemAddress __state
    ){
        VicinityTakeFinalize.OnMoveSucceeded(item, to, simulate, __result.Succeeded);
        VicinityListedWorldCleanup.TryCleanupAfterInventoryMutation(item, simulate, __result.Succeeded);
        VicinityListedWorldCleanup.TryCleanupAfterMoveFromAddress(__state, simulate, __result.Succeeded);
    }
}
