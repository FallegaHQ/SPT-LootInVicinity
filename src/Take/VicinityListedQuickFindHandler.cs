using EFT.Communications;
using EFT.InventoryLogic;
using EFT.UI;

namespace Softwyx.LootInVicinity.Take;

/// <summary>
/// Routes Ctrl+quick-move for listed world rows to player equipment. Vanilla
/// <see cref="ItemUiContext.QuickFindAppropriatePlace"/> picks <c>compoundItem_0</c> (the vicinity stash) when the
/// item is not a child of that stash, which leaves world-listed rows with no valid destination.
/// </summary>
internal static class VicinityListedQuickFindHandler{
    private static readonly CompoundItem[] EquipmentTargets = new CompoundItem[1];

    /// <param name="itemContext"></param>
    /// <param name="controller"></param>
    /// <param name="forcePutInStash"></param>
    /// <param name="displayWarnings"></param>
    /// <param name="simulate"></param>
    /// <param name="result"></param>
    /// <returns>Whether vanilla <see cref="ItemUiContext.QuickFindAppropriatePlace"/> should run.</returns>
    public static bool TryQuickFindListedWorldItemToPlayer(
        ItemContextAbstractClass itemContext, TraderControllerClass controller, bool forcePutInStash,
        bool                     displayWarnings, bool                  simulate,   out ItemUiQuickFindResult result
    ){
        result = default;

        var item = itemContext?.Item;

        if(item == null || forcePutInStash || !VicinityLootSession.HasListedWorldBinding(item)) return true;

        if(!IsVicinityPanelController(controller)) return true;

        var inventoryController = controller as InventoryController ?? VicinityLocalPlayer.InventoryController;
        var equipment           = inventoryController?.Inventory?.Equipment;

        if(equipment == null) return true;

        EquipmentTargets[0] = equipment;

        var order = InteractionsHandlerClass.EMoveItemOrder.MoveToAnotherSide
                  | InteractionsHandlerClass.EMoveItemOrder.IgnoreItemParent;

        var quickFind = InteractionsHandlerClass.QuickFindAppropriatePlace(
                                                                         item,
                                                                         inventoryController,
                                                                         EquipmentTargets,
                                                                         order,
                                                                         simulate
                                                                        );

        if(quickFind.Failed && displayWarnings) DisplayQuickFindWarning(quickFind);

        result = quickFind;

        return false;
    }

    private static bool IsVicinityPanelController(TraderControllerClass itemController){
        if(itemController == null) return false;

        return itemController == VicinityRaidServices.VicinityTrader
            || VicinityLocalPlayer.MatchesInventoryController(itemController as InventoryController);
    }

    private static void DisplayQuickFindWarning(QuickFindResult result){
        if(!result.Failed) return;

        var text           = result.Error is InventoryError inventoryError ? inventoryError.GetLocalizedDescription() : result.Error.ToString();

        NotificationManagerClass.DisplayWarningNotification(text.Localized());
    }
}
