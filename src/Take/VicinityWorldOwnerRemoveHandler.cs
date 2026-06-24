using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

namespace Softwyx.LootInVicinity.Take;

/// <summary>Routes world <see cref="IItemOwner" /> remove events for listed vicinity loot into take or unlist logic.</summary>
internal static class VicinityWorldOwnerRemoveHandler{
    public static void OnRemoveItem(RemoveItemEventArgs args){
        if(args is null) return;

        if(args.Status != CommandStatus.Succeed) return;

        if(VicinityLootSession.IsPopulating) return;

        if(!VicinityLootSession.HasListedWorldBinding(args.Item)) return;

        if(args.Location != null && VicinityStashGrid.IsVicinityStashAddress(args.Location)) return;

        if(args.Location != null && VicinityLootSession.ItemStillOnWorldLootAfterRemove(args.Item, args.Location))
            return;

        if(Singleton<GameWorld>.Instantiated && !string.IsNullOrEmpty(args.OwnerId)){
            var owner = Singleton<GameWorld>.Instance.FindOwnerById(args.OwnerId);

            if(owner != null) VicinityItemOwnerEvents.RemoveRemoveHandler(owner, OnRemoveItem);
        }

        if(VicinityPlayerInventory.IsInLocalPlayerInventory(args.Item)
        || (VicinityListedLootRegistry.TryGetWorldLoot(args.Item, out var worldLoot)
         && worldLoot.Item is{
                                 StackObjectsCount: <= 0
                             })){
            VicinityTakeFinalize.TryFinalizeListedTake(args.Item, args.Location);

            return;
        }

        VicinityLootSession.UnlistFromPanelWithoutDestroyingWorld(args.Item);
    }
}
