using EFT.InventoryLogic;
using System.Collections;

namespace Softwyx.LootInVicinity.Take;

/// <summary>Shared take-finalization gates used by Move / QuickFind / ItemUiContext patches.</summary>
internal static class VicinityTakeFinalize{
    /// <summary>
    /// Identifies move/quick-find sources for the fake vicinity stash or local raid inventory.
    /// </summary>
    /// <param name="itemController"></param>
    /// <returns>Whether the controller is <see cref="VicinityRaidServices.VicinityTrader"/> or the local player.</returns>
    public static bool IsVicinityPanelController(TraderControllerClass itemController){
        if(itemController == null) return false;

        return itemController == VicinityRaidServices.VicinityTrader
            || VicinityLocalPlayer.MatchesInventoryController(itemController as InventoryController);
    }

    /// <summary>
    /// Central gate before <see cref="VicinityLootSession.ScheduleTakeFromPanel"/> runs from patch or world-owner paths.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="to">Destination after move; ignored when null.</param>
    /// <returns>Whether this item should run vicinity take cleanup now.</returns>
    public static bool ShouldRunTakeCleanup(Item item, ItemAddress to = null){
        if(item == null) return false;

        if(VicinityPanelPresenter.IsInventoryClosing) return false;

        if(!VicinityLifecycle.PanelTakeCleanupActive || UiAccess.IsWorldContainerLootOpen()) return false;

        if(to != null && VicinityStashGrid.IsVicinityStashAddress(to)) return false;

        if(VicinityLootSession.HasListedWorldBinding(item)) return true;

        var grid = VicinityLootSession.GetVicinityGrid();

        return grid != null && grid.Contains(item);
    }

    /// <summary>
    /// Removes the vicinity row and schedules world destroy after a successful take. Defers one frame when
    /// <see cref="LootInVicinityPlugin.Instance"/> exists so other mods postfixes (e.g. UIFixes) run first.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destinationAfterTake">Player inventory address after take; falls back to <see cref="Item.CurrentAddress"/>.</param>
    public static void OnMoveSucceeded(Item item, ItemAddress to, bool simulate, bool succeeded){
        if(simulate || !succeeded) return;

        TryFinalizeListedTake(item, to);
    }

    public static void OnHandlerQuickFindSucceeded(
        Item item, TraderControllerClass controller, bool simulate, bool succeeded
    ){
        if(simulate || item == null || !succeeded || !IsVicinityPanelController(controller)) return;

        TryFinalizeListedTake(item, item.CurrentAddress);
    }

    public static void OnUiQuickFindSucceeded(
        ItemContextAbstractClass itemContext, TraderControllerClass controller, bool simulate, bool failed
    ){
        if(simulate || itemContext?.Item == null || failed || !IsVicinityPanelController(controller)) return;

        TryFinalizeListedTake(itemContext.Item, itemContext.Item.CurrentAddress);
    }

    public static void ApplyListedQuickFindFlags(
        Item item, TraderControllerClass controller, ref InteractionsHandlerClass.EMoveItemOrder order
    ){
        if(item == null || !VicinityLootSession.HasListedWorldBinding(item)) return;

        if(!IsVicinityPanelController(controller)) return;

        order |= InteractionsHandlerClass.EMoveItemOrder.IgnoreItemParent;
    }

    public static void TryFinalizeListedTake(Item item, ItemAddress destinationAfterTake = null){
        if(item == null || !ShouldRunTakeCleanup(item, destinationAfterTake)) return;

        var destination = destinationAfterTake ?? item.CurrentAddress;

        if(destination != null && VicinityStashGrid.IsVicinityStashAddress(destination)) return;

        if(!LootInVicinityPlugin.Instance){
            VicinityLootSession.ScheduleTakeFromPanel(item, destination);

            return;
        }

        LootInVicinityPlugin.Instance.StartCoroutine(DeferredFinalizeListedTakeRoutine(item, destination));
    }

    /// <summary>
    /// One-frame delay before <see cref="VicinityLootSession.ScheduleTakeFromPanel"/>.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destination"></param>
    /// <returns>Yields one frame, then schedules take cleanup if gates still pass.</returns>
    private static IEnumerator DeferredFinalizeListedTakeRoutine(Item item, ItemAddress destination){
        yield return null;

        if(!ShouldRunTakeCleanup(item, destination)) yield break;

        VicinityLootSession.ScheduleTakeFromPanel(item, destination);
    }
}
