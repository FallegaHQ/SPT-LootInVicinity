using System.Collections;
using System.Linq;
using EFT.Interactive;
using EFT.InventoryLogic;

namespace Softwyx.LootInVicinity.World;

/// <summary>
///     Removes vicinity panel rows and world <see cref="LootItem" /> representations when listed loot is consumed,
///     discarded, emptied, or otherwise no longer valid on the ground.
/// </summary>
internal static class VicinityListedWorldCleanup{
    public static bool IsListedWorldCleanupActive =>
        Settings.Enabled.Value
     && VicinityLifecycle.PanelTakeCleanupActive
     && !VicinityPanelPresenter.IsInventoryClosing;

    public static bool IsWorldRepresentationObsolete(Item item){
        return item switch{
                   null
                     or{
                           StackObjectsCount: <= 0
                       }
                    or IAmmoContainer{
                           Count: <= 0
                       }
                    or MedsItemClass{
                           MedKitComponent.HpResource: <= 0
                       }
                    or FoodDrinkItemClass{
                           FoodDrinkComponent.HpPercent: <= 0
                       } => true,
                   _ => false
               };
    }

    /// <param name="item"></param>
    public static void TryCleanupOnItemRefresh(Item item){
        TryCleanupIfObsoletePanelItem(item);
    }

    /// <param name="item"></param>
    public static void ScheduleObsoleteListedWorldCleanup(Item item){
        if(item == null || !IsListedWorldCleanupActive) return;

        if(!LootInVicinityPlugin.Instance){
            TryCleanupOnItemRefresh(item);

            return;
        }

        LootInVicinityPlugin.Instance.StartCoroutine(DeferredObsoleteListedWorldCleanupRoutine(item));
    }

    private static IEnumerator DeferredObsoleteListedWorldCleanupRoutine(Item item){
        yield return null;

        if(item != null) TryCleanupOnItemRefresh(item);
    }

    /// <param name="item"></param>
    /// <param name="simulate"></param>
    /// <param name="succeeded"></param>
    public static void TryCleanupAfterInventoryMutation(Item item, bool simulate, bool succeeded){
        if(simulate || !succeeded || item == null) return;

        TryCleanupIfObsoletePanelItem(item);
    }

    /// <param name="fromAddress">Source address captured before a move.</param>
    /// <param name="simulate"></param>
    /// <param name="succeeded"></param>
    public static void TryCleanupAfterMoveFromAddress(ItemAddress fromAddress, bool simulate, bool succeeded){
        if(!IsListedWorldCleanupActive || simulate || !succeeded || fromAddress == null) return;

        var parentItems = fromAddress.GetAllParentItems().
                                      ToList();

        foreach(var parentItem in parentItems) TryCleanupIfObsoletePanelItem(parentItem);
    }

    public static void TryCleanupOnDestroyLoot(LootItem worldLoot, Item item){
        if(!worldLoot || item == null) return;

        if(VicinityTakeCleanup.IsPendingTake(item.Id)) return;

        if(VicinityPlayerInventory.IsInLocalPlayerInventory(item)){
            VicinityLootSession.DestroyWorldLootGameObjectOnly(worldLoot);

            return;
        }

        TryCleanupListedWorldRepresentation(item, worldLoot);
    }

    public static void TryCleanupListedWorldRepresentation(Item item, LootItem worldLoot = null){
        if(item == null) return;

        var hadBinding = VicinityLootSession.HasListedWorldBinding(item);
        var inGrid = VicinityLootSession.GetVicinityGrid()?.
                                         Contains(item)
                  == true;
        var staged = VicinityStagingRegistry.IsStaged(item);

        if(!hadBinding && !inGrid && !staged) return;

        if(!worldLoot) VicinityListedLootRegistry.TryGetWorldLoot(item, out worldLoot);

        VicinityLootSession.GetVicinityGrid()?.
                            ForceRemoveListedItem(item);

        if(hadBinding){
            VicinityListedLootRegistry.Remove(item.Id);
            VicinityListedLootRegistry.UnsubscribeWorldOwner(item);
        }

        if(staged) VicinityStagingRegistry.Unregister(item);

        if(worldLoot) VicinityLootSession.DestroyWorldLootGameObjectOnly(worldLoot);
    }

    private static void TryCleanupIfObsoletePanelItem(Item item){
        if(item == null || !IsListedWorldCleanupActive) return;

        if(VicinityTakeCleanup.IsPendingTake(item.Id)) return;

        if(VicinityPlayerInventory.IsInLocalPlayerInventory(item) && VicinityLootSession.HasListedWorldBinding(item))
            return;

        if(!ShouldCleanupPanelItem(item)) return;

        if(!IsWorldRepresentationObsolete(item)) return;

        TryCleanupListedWorldRepresentation(item);
    }

    private static bool ShouldCleanupPanelItem(Item item){
        if(VicinityLootSession.HasListedWorldBinding(item) || VicinityStagingRegistry.IsStaged(item)) return true;

        var grid = VicinityLootSession.GetVicinityGrid();

        return grid != null && grid.Contains(item);
    }
}
