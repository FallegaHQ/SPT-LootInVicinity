using System.Diagnostics.CodeAnalysis;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Prefix on <see cref="InteractionsHandlerClass.Move"/> for quest items --
/// delegates to <see cref="QuestItemMoveHandler"/>.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class QuestItemMovePatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(InteractionsHandlerClass),
                                  nameof(InteractionsHandlerClass.Move),
                                  [typeof(Item), typeof(ItemAddress), typeof(TraderControllerClass), typeof(bool)]
                                 );
    }

    [PatchPrefix]
    public static bool PatchPrefix(
        Item item, ItemAddress to, TraderControllerClass itemController, bool simulate, ref MoveResult __result
    ){
        return QuestItemMoveHandler.TryInterceptMove(item, to, itemController, simulate, out __result);
    }
}
