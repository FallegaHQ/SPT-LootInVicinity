using System.Collections.Generic;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using Softwyx.LootInVicinity.Config;
using Softwyx.LootInVicinity.LivPlayer;
using Softwyx.LootInVicinity.Raid;
using Softwyx.LootInVicinity.Session;
using StashGridClass = EFT.InventoryLogic.Grid;


namespace Softwyx.LootInVicinity.Grid;

internal class VicinityStashGrid(string id, CompoundItem parentItem)
    : StashGridClass(id, 10, 12, true, false, [], parentItem){
    public GridView[] GridViews{
        get;
        set;
    }

    public override GridItemCollection ItemCollection{
        get;
    } = new VicinityStashGridCollection();

    public static bool IsVicinityStashAddress(ItemAddress address){
        if(address?.Container == null || VicinityRaidServices.RadiusStash == null) return false;

        if(address.Container.ParentItem == VicinityRaidServices.RadiusStash) return true;

        return address.Container is VicinityStashGrid;
    }

    public override bool CheckCompatibility(Item item){
        if(item == null || Contains(item)) return false;

        if(VicinityLootSession.HasListedWorldBinding(item)) return VicinityLootSession.IsShownInVicinityPanel(item);

        return Settings.AllowVicinityStaging.Value && base.CheckCompatibility(item);
    }

    internal LocationInGrid FindFreeSpaceForListing(Item item){
        if(item == null) return null;

        if(VicinityPlayerInventory.IsInLocalPlayerInventory(item)) return null;

        return Contains(item) ? null : base.FindFreeSpace(item);
    }

    public override LocationInGrid FindFreeSpace(Item item){
        if(item == null) return null;

        if(!VicinityLootSession.HasListedWorldBinding(item))
            return Settings.AllowVicinityStaging.Value ? base.FindFreeSpace(item) : null;

        return !VicinityLootSession.IsShownInVicinityPanel(item) ? null : base.FindFreeSpace(item);
    }

    public override OperationResult<GridAddResult> AddInternal(
        Item item, LocationInGrid location, bool simulate, bool ignoreRestrictions
    ){
        if(UsesRealInventoryMove(item, ignoreRestrictions))
            return base.AddInternal(item, location, simulate, ignoreRestrictions);

        if(location == null)
            return new OperationResult<GridAddResult>(new WontFitToGridError(item, null, this));

        if(item == null) return new OperationResult<GridAddResult>(new PlaceTakenByAnotherItemError(null, location, this));

        if(!ignoreRestrictions && !CheckCompatibility(item))
            return new OperationResult<GridAddResult>(new PlaceTakenByAnotherItemError(item, location, this));

        var address    = CreateItemAddress(location);
        var stackCount = item.StackObjectsCount;

        if(simulate)
            return new OperationResult<GridAddResult>(
                                                      new GridAddResult(
                                                                        this,
                                                                        item,
                                                                        address,
                                                                        stackCount,
                                                                        null,
                                                                        true
                                                                       )
                                                     );

        PlaceItem(item, location);

        return new OperationResult<GridAddResult>(
                                                 new GridAddResult(
                                                                   this,
                                                                   item,
                                                                   address,
                                                                   stackCount,
                                                                   null,
                                                                   false
                                                                  )
                                                );
    }

    public override OperationResult<ContainerRemoveResult> RemoveInternal(Item item, bool simulate, bool ignoreRestrictions){
        if(UsesRealInventoryRemove(item)) return base.RemoveInternal(item, simulate, ignoreRestrictions);

        if(!Contains(item)) return new OperationResult<ContainerRemoveResult>(new NoFreeSpaceError(item, this));

        var locationInGrid = ItemCollection[item];
        var fromAddress    = CreateItemAddress(locationInGrid);

        if(!simulate) RemoveItem(item, locationInGrid);

        return new OperationResult<ContainerRemoveResult>(new ContainerRemoveResult(item, fromAddress, simulate));
    }

    private void DetachListedItem(Item item){
        if(item == null || !Contains(item)) return;

        var locationInGrid = ItemCollection[item];

        RemoveItem(item, locationInGrid);
    }

    public void ForceRemoveListedItem(Item item){
        if(item == null) return;

        ItemAddress fromAddress;

        if(Contains(item)){
            var locationInGrid = ItemCollection[item];

            fromAddress = CreateItemAddress(locationInGrid);
            DetachListedItem(item);
        }
        else{
            try{
                fromAddress = item.Parent;
            }
            catch{
                fromAddress = item.CurrentAddress;
            }
        }

        NotifyGridViewsItemRemoved(item, fromAddress);
    }

    private void NotifyGridViewsItemRemoved(Item item, ItemAddress fromAddress){
        if(GridViews == null || GridViews.Length == 0) return;

        var owner = VicinityRaidServices.VicinityTrader;

        if(owner == null) return;

        foreach(var gridView in GridViews){
            if(!gridView) continue;

            ((IRemoveHandler)gridView).OnItemRemoved(new RemoveItemEventArgs(item, fromAddress, CommandStatus.Begin,   owner));
            ((IRemoveHandler)gridView).OnItemRemoved(new RemoveItemEventArgs(item, fromAddress, CommandStatus.Succeed, owner));
        }
    }

    private static bool UsesRealInventoryMove(Item item, bool ignoreRestrictions){
        if(!Settings.AllowVicinityStaging.Value || item == null) return false;

        switch(ignoreRestrictions){
            case true when VicinityLootSession.HasListedWorldBinding(item):
            case true:
                return false;
        }

        if(VicinityStagingRegistry.IsStaged(item)) return true;

        var owner = item.CurrentAddress?.GetOwnerOrNull();

        if(owner == VicinityRaidServices.VicinityTrader) return true;

        return item.CurrentAddress == null || VicinityPlayerInventory.IsInLocalPlayerInventory(item);
    }

    private static bool UsesRealInventoryRemove(Item item){
        if(!Settings.AllowVicinityStaging.Value || item == null) return false;

        if(VicinityStagingRegistry.IsStaged(item)) return true;

        return item.CurrentAddress?.GetOwnerOrNull() == VicinityRaidServices.VicinityTrader;
    }

    private sealed class VicinityStashGridCollection : GridItemCollection{
        private Dictionary<Item, LocationInGrid> ItemLocations => Items;

        public override void Add(Item item, StashGridClass grid, LocationInGrid location){
            if(item == null) return;

            if(UsesRealInventoryMove(item, false)){
                base.Add(item, grid, location);

                return;
            }

            ItemLocations[item] = location;
            ItemsList.Add(item);
        }

        public override void Remove(Item item, StashGridClass grid){
            if(item == null) return;

            if(UsesRealInventoryRemove(item)){
                base.Remove(item, grid);

                return;
            }

            ItemLocations.Remove(item);
            ItemsList.Remove(item);
        }
    }
}
