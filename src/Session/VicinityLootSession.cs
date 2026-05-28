using EFT.Interactive;
using EFT.InventoryLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Softwyx.LootInVicinity.Session;

/// <summary>Raid listing orchestration: fake stash populate, trader hook, and session flags.</summary>
internal static class VicinityLootSession{
    public const string OwnerId = "com.softwyx.lootinvicinity.trader";

    private static Action<RemoveItemEventArgs> _traderRemoveHandler;
    private static bool                        _suppressWorldSync;

    public static bool IsPopulating{
        get;
        private set;
    }

    internal static bool SuppressWorldSync => _suppressWorldSync;

    public static int ListedCount => VicinityListedLootRegistry.Count;

    public static void SetPopulating(bool populating){
        IsPopulating = populating;
    }

    public static bool HasListedWorldBinding(Item item){
        return VicinityListedLootRegistry.HasBinding(item);
    }

    public static bool IsShownInVicinityPanel(Item item){
        if(!HasListedWorldBinding(item)) return false;

        var grid = GetVicinityGrid();

        return grid != null && grid.Contains(item);
    }

    public static bool HasLeftVicinityStash(Item item){
        return HasListedWorldBinding(item) && !IsShownInVicinityPanel(item);
    }

    public static bool ItemStillOnWorldLootAfterRemove(Item item, ItemAddress destination){
        return VicinityListedLootRegistry.ItemStillOnWorldLootAfterRemove(item, destination);
    }

    public static VicinityStashGrid GetVicinityGrid(){
        var stash = VicinityRaidServices.RadiusStash;

        return stash?.Grids is{
                                  Length: > 0
                              }
                   ? stash.Grids[0] as VicinityStashGrid
                   : null;
    }

    public static void BindTraderEvents(TraderControllerClass trader){
        if(trader == null) return;

        UnbindTraderEvents(trader);
        _traderRemoveHandler ??= OnTraderRemoveItem;
        VicinityItemOwnerEvents.AddRemoveHandler(trader, _traderRemoveHandler);
    }

    public static void UnbindTraderEvents(TraderControllerClass trader){
        if(trader == null || _traderRemoveHandler == null) return;

        VicinityItemOwnerEvents.RemoveRemoveHandler(trader, _traderRemoveHandler);
    }

    public static void ResetRaidState(){
        UnbindTraderEvents(VicinityRaidServices.VicinityTrader);
        VicinityListedLootRegistry.Clear();
        VicinityTakeCleanup.ClearPendingTakes();
    }

    public static void ClearStashContents(){
        var grid = GetVicinityGrid();

        _suppressWorldSync = true;

        try{
            if(grid != null){
                foreach(var item in grid.Items.ToList()){
                    VicinityListedLootRegistry.UnsubscribeWorldOwner(item);
                    grid.ForceRemoveListedItem(item);
                }

                grid.GridViews = null;
            }

            VicinityListedLootRegistry.UnsubscribeAllWorldOwners();
            VicinityListedLootRegistry.Clear();
            VicinityTakeCleanup.ClearPendingTakes();
        }
        finally{
            _suppressWorldSync = false;
        }
    }

    public static IEnumerator PlaceCandidatesRoutine(
        IReadOnlyList<LootItem> candidates, Action<int> onComplete, bool clearStashFirst = true
    ){
        if(clearStashFirst) ClearStashContents();

        var grid = GetVicinityGrid();

        if(grid == null || candidates == null || candidates.Count == 0){
            onComplete?.Invoke(0);

            yield break;
        }

        yield return null;

        var placed     = 0;
        var sinceYield = 0;

        foreach(var lootItem in candidates){
            if(TryListWorldItem(grid, lootItem)) placed++;

            sinceYield++;

            if(sinceYield < 6) continue;

            sinceYield = 0;

            yield return null;
        }

        FinalizeGridLayout(grid);

        onComplete?.Invoke(placed);
    }

    public static void ScheduleTakeFromPanel(Item item, ItemAddress destinationAfterTake = null){
        VicinityTakeCleanup.ScheduleTakeFromPanel(item, destinationAfterTake);
    }

    public static void CompleteTakeFromPanel(Item item, ItemAddress destinationAfterTake = null){
        VicinityTakeCleanup.CompleteTakeFromPanel(item, destinationAfterTake);
    }

    public static void UnlistFromPanelWithoutDestroyingWorld(Item item){
        VicinityTakeCleanup.UnlistFromPanelWithoutDestroyingWorld(item);
    }

    internal static void DestroyWorldLootRepresentation(LootItem worldLoot, Item takenItem){
        VicinityTakeCleanup.DestroyWorldLootRepresentation(worldLoot, takenItem);
    }

    internal static void DestroyWorldLootGameObjectOnly(LootItem worldLoot){
        VicinityTakeCleanup.DestroyWorldLootGameObjectOnly(worldLoot);
    }

    internal static void TryRevalidateGrid(VicinityStashGrid grid){
        if(grid == null) return;

        try{
            grid.RevalidateSpaceBuffer();
        }
        catch{
            // ignored
        }
    }

    /// <summary>
    /// !! IMPORTANT !! To any dev who wants to extend this mod
    /// <see cref="VicinityItemOwnerEvents"/> handler on <see cref="VicinityRaidServices.VicinityTrader"/>. Intentionally
    /// empty.
    /// </summary>
    /// <remarks>
    /// Subscribed in <see cref="BindTraderEvents"/>. Dragging or quick-moving from the vicinity panel removes the row
    /// from the fake trader grid at move start, which raises this callback before the item is in player inventory and
    /// while <see cref="ItemAddress"/> may still point at the vicinity stash.
    /// Do not call <see cref="ScheduleTakeFromPanel"/> or <see cref="CompleteTakeFromPanel"/> here. Early clean-up
    /// drops the ListedToWorld binding before move or quick-find succeeds and causes ghost UI rows or a broken item.
    /// When the take actually completes, use <see cref="VicinityTakeFinalize.TryFinalizeListedTake"/> from
    /// <see cref="Softwyx.LootInVicinity.Patches.VicinityInteractionsMovePatch"/>,
    /// <see cref="Softwyx.LootInVicinity.Patches.VicinityQuickFindPatch"/>,
    /// <see cref="Softwyx.LootInVicinity.Patches.VicinityItemUiQuickFindPatch"/>, or
    /// <see cref="VicinityWorldOwnerRemoveHandler.OnRemoveItem"/>.
    /// </remarks>
    /// <param name="args"></param>
    private static void OnTraderRemoveItem(RemoveItemEventArgs args){}

    private static bool TryListWorldItem(VicinityStashGrid grid, LootItem worldLoot){
        var item = worldLoot?.Item;

        if(item?.Template == null || worldLoot.ItemOwner == null) return false;

        if(VicinityPlayerInventory.IsInLocalPlayerInventory(item)) return false;

        if(HasListedWorldBinding(item) && grid.Contains(item)) return false;

        if(item.Parent?.Container?.ID == grid.ID) return false;

        if(grid.Contains(item)) return false;

        var location = grid.FindFreeSpaceForListing(item);

        if(location == null) return false;

        VicinityListedLootRegistry.SubscribeWorldOwner(worldLoot);

        try{
            var addResult = grid.AddInternal(item, location, false, true);

            if(!addResult.Succeeded){
                VicinityListedLootRegistry.UnsubscribeWorldLoot(worldLoot);

                return false;
            }
        }
        catch(Exception){
            VicinityListedLootRegistry.UnsubscribeWorldLoot(worldLoot);

            return false;
        }

        VicinityListedLootRegistry.Register(item, worldLoot);

        return true;
    }

    private static void FinalizeGridLayout(VicinityStashGrid grid){
        TryRevalidateGrid(grid);
    }
}
