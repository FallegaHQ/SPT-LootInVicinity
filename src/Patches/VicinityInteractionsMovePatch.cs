using System.Diagnostics.CodeAnalysis;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Postfix on <see cref="InteractionsHandlerClass.Move"/> --
/// delegates to <see cref="VicinityTakeFinalize.OnMoveSucceeded"/>.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityInteractionsMovePatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(InteractionsHandlerClass),
                                  nameof(InteractionsHandlerClass.Move),
                                  [typeof(Item), typeof(ItemAddress), typeof(TraderControllerClass), typeof(bool)]
                                 );
    }

    [PatchPostfix]
    public static void PatchPostfix(
        Item item, ItemAddress to, TraderControllerClass itemController, bool simulate, ref MoveResult __result
    ){
        VicinityTakeFinalize.OnMoveSucceeded(item, to, simulate, __result.Succeeded);
    }
}
