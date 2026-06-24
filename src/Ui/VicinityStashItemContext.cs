using EFT.InventoryLogic;
using EFT.UI;

namespace Softwyx.LootInVicinity.Ui;

internal static class VicinityStashItemContext{
    public static ItemContextAbstractClass Create(
        ItemContextAbstractClass sourceContext, CompoundItem stash, SimpleStashPanel panelHost
    ){
        if(stash == null) return null;

        if(panelHost != null){
            var transferRoot = new TransferItemContext(EItemViewType.InventoryWithoutDiscard, panelHost);

            return transferRoot.CreatePlayerSideChild(stash);
        }

        if(sourceContext != null) return sourceContext.CreateChild(stash);

        var inventory = VicinityLocalPlayer.Inventory;

        if(inventory?.Equipment == null) return null;

        var root = new RaidInventoryItemContext(
                                                inventory.Equipment,
                                                GClass3459.EItemType.Inventory,
                                                inventory.FavoriteItemsStorage,
                                                false
                                               );

        return root.CreateChild(stash);
    }
}
