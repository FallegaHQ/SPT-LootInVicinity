using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;

namespace Softwyx.LootInVicinity.Quest;

internal static class QuestLootRouting{
    private static MethodInfo _quickFindMethod;

    /// <summary>
    /// Quick-moves a listed quest item into <see cref="Inventory.QuestRaidItems"/> via reflected
    /// <see cref="InteractionsHandlerClass.QuickFindAppropriatePlace"/>. Used by
    /// <see cref="Softwyx.LootInVicinity.Patches.QuestItemMovePatch"/>.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="controller"></param>
    /// <param name="simulate"></param>
    /// <param name="operation"></param>
    /// <returns>Whether quick-find into quest raid items inventory succeeded.</returns>
    public static bool TryMoveToQuestRaid(
        Item item, InventoryController controller, bool simulate, out QuickFindResult operation
    ){
        operation = default;

        if(item is not{
                          QuestItem: true
                      }
        || controller?.Inventory?.QuestRaidItems == null)
            return false;

        var questRaid = controller.Inventory.QuestRaidItems;
        var method    = ResolveQuickFindMethod();

        if(method == null) return false;

        operation = (QuickFindResult) method.Invoke(
                                                    null,
                                                    [
                                                        item,
                                                        controller,
                                                        new[]{
                                                                 questRaid
                                                             },
                                                        InteractionsHandlerClass.EMoveItemOrder.Apply,
                                                        simulate
                                                    ]
                                                   );

        return operation.Succeeded;
    }

    private static MethodInfo ResolveQuickFindMethod(){
        if(_quickFindMethod != null) return _quickFindMethod;

        _quickFindMethod = AccessTools.Method(
                                              typeof(InteractionsHandlerClass),
                                              nameof(InteractionsHandlerClass.QuickFindAppropriatePlace)
                                             );

        return _quickFindMethod;
    }
}
