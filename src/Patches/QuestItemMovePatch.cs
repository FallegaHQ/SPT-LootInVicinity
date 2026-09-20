using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using HarmonyLib;
using Softwyx.LootInVicinity.Quest;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Prefix on <see cref="EFT.InventoryLogic.ItemManipulator.Move" /> for quest items --
///     delegates to <see cref="QuestItemMoveHandler" />.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class QuestItemMovePatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(ItemManipulator),
                                  nameof(ItemManipulator.Move),
                                  [typeof(Item), typeof(ItemAddress), typeof(ItemController), typeof(bool)]
                                 );
    }

    [PatchPrefix]
    public static bool PatchPrefix(
        Item item, ItemAddress to, ItemController itemController, bool simulate, ref OperationResult<MoveResult> __result
    ){
        return QuestItemMoveHandler.TryInterceptMove(item, to, itemController, simulate, out __result);
    }
}
