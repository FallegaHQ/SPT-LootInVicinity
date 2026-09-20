using EFT.InventoryLogic;
using EFT.UI;
using Softwyx.LootInVicinity.LivPlayer;

namespace Softwyx.LootInVicinity.Ui;

internal static class VicinityStashItemContext{
    public static ItemContext Create(
        ItemContext sourceContext, CompoundItem stash, SimpleStashPanel panelHost
    ){
        if(stash == null) return null;

        if(panelHost != null){
            var transferRoot = new ReferenceItemContext(EItemViewType.InventoryWithoutDiscard, panelHost);

            return transferRoot.CreatePlayerSideChild(stash);
        }

        if(sourceContext != null) return sourceContext.CreateChild(stash);

        var inventory = VicinityLocalPlayer.Inventory;

        if(inventory?.Equipment == null) return null;

        var root = new AreaStashItemContext(
                                                inventory.Equipment,
                                                AreaStashItemContext.EItemType.Inventory,
                                                inventory.FavoriteItemsStorage,
                                                false
                                               );

        return root.CreateChild(stash);
    }
}
