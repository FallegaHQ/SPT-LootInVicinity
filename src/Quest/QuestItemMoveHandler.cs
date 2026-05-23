using EFT.InventoryLogic;
using Softwyx.LootInVicinity.LivPlayer;

namespace Softwyx.LootInVicinity.Quest;

/// <summary>Quest item drag from vicinity panel -- reroute to quest raid stash.</summary>
internal static class QuestItemMoveHandler{
    /// <param name="item"></param>
    /// <param name="to"></param>
    /// <param name="itemController"></param>
    /// <param name="simulate"></param>
    /// <param name="result"></param>
    /// <returns>Whether vanilla <see cref="InteractionsHandlerClass.Move"/> should run.</returns>
    public static bool TryInterceptMove(
        Item item, ItemAddress to, TraderControllerClass itemController, bool simulate, out MoveResult result
    ){
        result = default;

        if(simulate || item == null || to == null || !item.QuestItem || !VicinityLifecycle.QuestRoutingActive)
            return true;

        if(!VicinityLootSession.HasListedWorldBinding(item)) return true;

        if(VicinityStashGrid.IsVicinityStashAddress(to)) return true;

        var inventoryController = itemController as InventoryController ?? VicinityLocalPlayer.InventoryController;

        if(inventoryController?.Inventory?.QuestRaidItems == null) return true;

        if(to.IsChildOf(inventoryController.Inventory.QuestRaidItems, false)) return true;

        if(!QuestLootRouting.TryMoveToQuestRaid(item, inventoryController, false, out var questOperation)) return true;

        result = (MoveResult) (object) questOperation;

        if(result.Succeeded) VicinityTakeFinalize.TryFinalizeListedTake(item, item.CurrentAddress);

        return false;
    }
}
