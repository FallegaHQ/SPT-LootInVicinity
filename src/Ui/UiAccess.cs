using EFT.InventoryLogic;
using EFT.UI;
using Softwyx.LootInVicinity.LivPlayer;
using UnityEngine;

namespace Softwyx.LootInVicinity.Ui;

internal static class UiAccess{
    private static bool _worldLootOpen;

    public static bool IsRaidItemsPanel(ItemsPanel itemsPanel){
        return itemsPanel && itemsPanel.GetType() == typeof(ItemsPanel);
    }

    public static void SetWorldLootOpen(bool open){
        _worldLootOpen = open;
    }

    public static void ResetRaidUiState(){
        _worldLootOpen = false;
        VicinityLocalPlayer.Clear();
    }

    public static bool IsWorldContainerLootOpen(){
        return _worldLootOpen;
    }

    public static bool IsLocalRaidInventory(InventoryController inventoryController){
        return VicinityLocalPlayer.MatchesInventoryController(inventoryController);
    }

    public static bool CanAttachVicinityPanel(
        ItemsPanel itemsPanel, CompoundItem lootItem, InventoryController inventoryController
    ){
        if(!IsRaidItemsPanel(itemsPanel)) return false;

        if(!Settings.ShouldShowVicinityPanel(lootItem != null)) return false;

        if(!IsLocalRaidInventory(inventoryController)) return false;

        if(VicinityUiReflection.IsComplexStashPanelVisible(itemsPanel)) return false;

        var panelInventory = VicinityUiReflection.GetPanelInventory(itemsPanel);

        return panelInventory == null || panelInventory == inventoryController.Inventory;
    }

    public static void ClearDragLayer(){
        var dragLayer = VicinityUiReflection.GetDragLayer();

        if(!dragLayer) return;

        for(var i = dragLayer.childCount - 1; i >= 0; i--)
            Object.Destroy(
                           dragLayer.GetChild(i).
                                     gameObject
                          );
    }

    public static AddViewListClass GetItemsPanelUi(ItemsPanel itemsPanel){
        return VicinityUiReflection.GetItemsPanelUi(itemsPanel);
    }
}
