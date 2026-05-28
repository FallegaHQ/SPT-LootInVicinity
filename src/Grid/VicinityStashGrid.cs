using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using System.Collections.Generic;

namespace Softwyx.LootInVicinity.Grid;

internal class VicinityStashGrid(string id, CompoundItem parentItem)
    : StashGridClass(id, 10, 12, true, false, [], parentItem){
    public GridView[] GridViews{
        get;
        set;
    }

    public override StashGridCollectionClass ItemCollection{
        get;
    } = new VicinityStashGridCollection();

    public static bool IsVicinityStashAddress(ItemAddress address){
        if(address?.Container == null || VicinityRaidServices.RadiusStash == null) return false;

        if(address.Container.ParentItem == VicinityRaidServices.RadiusStash) return true;

        return address.Container is VicinityStashGrid;
    }

    public override bool CheckCompatibility(Item item){
        if(item == null) return false;

        return !Contains(item) && VicinityLootSession.IsShownInVicinityPanel(item);
    }

    internal LocationInGrid FindFreeSpaceForListing(Item item){
        if(item == null) return null;

        if(VicinityPlayerInventory.IsInLocalPlayerInventory(item)) return null;

        return Contains(item) ? null : base.FindFreeSpace(item);
    }

    public override LocationInGrid FindFreeSpace(Item item){
        if(item == null || !VicinityLootSession.IsShownInVicinityPanel(item)) return null;

        return base.FindFreeSpace(item);
    }

    public override ContainerAddEventResultStruct AddInternal(
        Item item, LocationInGrid location, bool simulate, bool ignoreRestrictions
    ){
        if(location == null)
            return new ContainerAddEventResultStruct(new GridNullLocationInventoryError(item, null, this));

        if(item == null) return new ContainerAddEventResultStruct(new GridFilterInventoryError(null, location, this));

        if(!ignoreRestrictions && !CheckCompatibility(item))
            return new ContainerAddEventResultStruct(new GridFilterInventoryError(item, location, this));

        var address    = CreateItemAddress(location);
        var stackCount = item.StackObjectsCount;

        if(simulate)
            return new ContainerAddEventResultStruct(
                                                     new ContainerAddEventClass(
                                                                                this,
                                                                                item,
                                                                                address,
                                                                                stackCount,
                                                                                null,
                                                                                true
                                                                               )
                                                    );

        PlaceItemInGrid(item, location);

        return new ContainerAddEventResultStruct(
                                                 new ContainerAddEventClass(
                                                                            this,
                                                                            item,
                                                                            address,
                                                                            stackCount,
                                                                            null,
                                                                            false
                                                                           )
                                                );
    }

    public override ContainerRemoveEventResultStruct RemoveInternal(Item item, bool simulate, bool ignoreRestrictions){
        if(!Contains(item)) return new ContainerRemoveEventResultStruct(new GridRemoveInventoryError(item, this));

        var locationInGrid = ItemCollection[item];
        var fromAddress    = CreateItemAddress(locationInGrid);

        if(!simulate) RemoveItemFromGrid(item, locationInGrid, true);

        return new ContainerRemoveEventResultStruct(new ContainerRemoveEventClass(item, fromAddress, simulate));
    }

    public void DetachListedItem(Item item){
        if(item == null || !Contains(item)) return;

        var locationInGrid = ItemCollection[item];

        RemoveItemFromGrid(item, locationInGrid, true);
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

            gridView.OnItemRemoved(new RemoveItemEventArgs(item, fromAddress, CommandStatus.Begin,   owner));
            gridView.OnItemRemoved(new RemoveItemEventArgs(item, fromAddress, CommandStatus.Succeed, owner));
        }
    }

    private void PlaceItemInGrid(Item item, LocationInGrid location){
        method_9(item, location);
    }

    private void RemoveItemFromGrid(Item item, LocationInGrid location, bool updateSpaceBuffer){
        method_10(item, location, updateSpaceBuffer);
    }

    private sealed class VicinityStashGridCollection : StashGridCollectionClass{
        private Dictionary<Item, LocationInGrid> ItemLocations => Dictionary_0;

        private List<Item> ItemList => List_0;

        public override void Add(Item item, StashGridClass grid, LocationInGrid location){
            if(item == null) return;

            ItemLocations[item] = location;
            ItemList.Add(item);
        }

        public override void Remove(Item item, StashGridClass grid){
            if(item == null) return;

            ItemLocations.Remove(item);
            ItemList.Remove(item);
        }
    }
}
