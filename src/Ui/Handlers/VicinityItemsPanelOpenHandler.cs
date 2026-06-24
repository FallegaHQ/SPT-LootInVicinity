using System.Collections;
using System.Threading.Tasks;
using EFT.InventoryLogic;
using EFT.UI;

namespace Softwyx.LootInVicinity.Ui.Handlers;

/// <summary>ItemsPanel.Show attach flow after tab inventory opens.</summary>
internal static class VicinityItemsPanelOpenHandler{
    private static int _openGeneration;

    public static void OnItemsPanelShow(
        ItemsPanel          itemsPanel, Task showTask, ItemContextAbstractClass sourceContext, CompoundItem lootItem,
        InventoryController inventoryController, ItemsPanel.EItemsTab currentTab
    ){
        if(itemsPanel == null || showTask == null || !Settings.Enabled.Value || !VicinityLifecycle.RaidSessionActive)
            return;

        if(inventoryController == null || !UiAccess.IsRaidItemsPanel(itemsPanel)) return;

        if(!UiAccess.IsLocalRaidInventory(inventoryController)) return;

        if(!VicinityPanelPresenter.TryGetSimpleStashPanel(itemsPanel, out var simpleStashPanel)) return;

        UiAccess.SetWorldLootOpen(lootItem != null);
        VicinityPanelPresenter.CancelAttachRoutine();

        if(lootItem != null || !UiAccess.CanAttachVicinityPanel(itemsPanel, null, inventoryController)){
            VicinityPanelPresenter.HideIfActive(itemsPanel);

            return;
        }

        VicinityRaidBootstrap.EnsureRaidStash();

        if(!VicinityRaidBootstrap.IsReady || simpleStashPanel == null) return;

        var generation = ++_openGeneration;
        var ui         = UiAccess.GetItemsPanelUi(itemsPanel);

        VicinityPanelPresenter.BeginAttachFromItemsPanel(
                                                         itemsPanel,
                                                         OpenAfterItemsPanelShow(
                                                              generation,
                                                              itemsPanel,
                                                              showTask,
                                                              simpleStashPanel,
                                                              inventoryController,
                                                              sourceContext,
                                                              currentTab,
                                                              ui
                                                             )
                                                        );
    }

    private static IEnumerator OpenAfterItemsPanelShow(
        int                  generation, ItemsPanel itemsPanel, Task showTask, SimpleStashPanel simpleStashPanel,
        InventoryController  inventoryController, ItemContextAbstractClass sourceContext,
        ItemsPanel.EItemsTab currentTab, AddViewListClass uiDisposableList
    ){
        while(!showTask.IsCompleted){
            if(generation != _openGeneration) yield break;

            yield return null;
        }

        if(generation != _openGeneration) yield break;

        if(showTask.IsFaulted || showTask.IsCanceled) yield break;

        yield return null;
        yield return null;

        if(generation != _openGeneration) yield break;

        if(UiAccess.IsWorldContainerLootOpen()) yield break;

        if(!UiAccess.CanAttachVicinityPanel(itemsPanel, null, inventoryController)) yield break;

        if(!UiAccess.IsLocalRaidInventory(inventoryController)) yield break;

        yield return VicinityPanelPresenter.AttachNearbyPanelRoutine(
                                                                     itemsPanel,
                                                                     simpleStashPanel,
                                                                     inventoryController,
                                                                     sourceContext,
                                                                     currentTab,
                                                                     uiDisposableList
                                                                    );
    }
}
