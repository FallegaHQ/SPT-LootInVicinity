using System;
using System.Collections;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using Object = UnityEngine.Object;

namespace Softwyx.LootInVicinity.Session;

/// <summary>Take completion, address sync, and deferred world-pile destroy after panel pickup.</summary>
internal static class VicinityTakeCleanup{
    private static readonly HashSet<string> PendingTakeIds = [];

    public static void ScheduleTakeFromPanel(Item item, ItemAddress destinationAfterTake = null){
        if(item == null) return;

        VicinityListedLootRegistry.TryGetWorldLoot(item, out var worldLoot);

        var inInventory = VicinityPlayerInventory.IsInLocalPlayerInventory(item);
        var depletedOnWorld = worldLoot?.Item is{
                                                    StackObjectsCount: <= 0
                                                };

        if(!inInventory && !depletedOnWorld) return;

        var grid = VicinityLootSession.GetVicinityGrid();

        if(!VicinityListedLootRegistry.HasBinding(item) && (grid == null || !grid.Contains(item))) return;

        if(!IsListedTakeComplete(item, worldLoot)) return;

        CompleteTakeFromPanel(item, destinationAfterTake);

        if(!worldLoot || LootInVicinityPlugin.Instance == null) return;

        if(!PendingTakeIds.Add(item.Id)) return;

        LootInVicinityPlugin.Instance.StartCoroutine(DeferredDestroyWorldLootRoutine(worldLoot, item));
    }

    private static void CompleteTakeFromPanel(Item item, ItemAddress destinationAfterTake = null){
        if(item == null) return;

        VicinityListedLootRegistry.TryGetWorldLoot(item, out var worldLoot);

        var grid = VicinityLootSession.GetVicinityGrid();

        grid?.ForceRemoveListedItem(item);

        TrySyncCurrentAddressAfterTake(item, destinationAfterTake);

        VicinityListedLootRegistry.Remove(item.Id);
        VicinityListedLootRegistry.UnsubscribeWorldLoot(worldLoot);

        VicinityLootSession.TryRevalidateGrid(grid);

        VicinityLootExperience.TryGrantForTake(item);
    }

    public static void UnlistFromPanelWithoutDestroyingWorld(Item item){
        if(item == null || !VicinityListedLootRegistry.Remove(item.Id)) return;

        VicinityListedLootRegistry.UnsubscribeWorldOwner(item);

        VicinityLootSession.GetVicinityGrid()?.
                            ForceRemoveListedItem(item);
    }

    public static void ClearPendingTakes(){
        PendingTakeIds.Clear();
    }

    public static bool IsPendingTake(string itemId){
        return !string.IsNullOrEmpty(itemId) && PendingTakeIds.Contains(itemId);
    }

    private static IEnumerator DeferredDestroyWorldLootRoutine(LootItem worldLoot, Item takenItem){
        yield return null;

        var itemId = takenItem?.Id;

        try{
            if(VicinityLootSession.SuppressWorldSync || VicinityLootSession.IsPopulating || !worldLoot) yield break;

            if(!Singleton<GameWorld>.Instantiated) yield break;

            if(takenItem != null
            && !VicinityPlayerInventory.IsInLocalPlayerInventory(takenItem)
            && worldLoot.Item is not{
                                        StackObjectsCount: <= 0
                                    })
                yield break;

            if(VicinityListedLootRegistry.OtherListedItemsShareWorldLoot(worldLoot, itemId)) yield break;

            DestroyWorldLootRepresentation(worldLoot, takenItem);
        }
        finally{
            if(!string.IsNullOrEmpty(itemId)) PendingTakeIds.Remove(itemId);
        }
    }

    private static bool IsListedTakeComplete(Item takenItem, LootItem worldLoot){
        if(takenItem == null) return false;

        if(worldLoot?.Item == null) return true;

        var worldItem = worldLoot.Item;

        if(worldItem.StackObjectsCount <= 0) return true;

        if(!ReferenceEquals(worldItem, takenItem)) return false;

        return VicinityPlayerInventory.IsInLocalPlayerInventory(takenItem);
    }

    private static void DestroyWorldLootRepresentation(LootItem worldLoot, Item takenItem){
        if(worldLoot?.Item == null) return;

        var worldItem = worldLoot.Item;

        if(worldItem.StackObjectsCount <= 0){
            DestroyWorldLootGameObjectOnly(worldLoot);

            return;
        }

        if(takenItem != null && !ReferenceEquals(worldItem, takenItem)) return;

        DestroyWorldLootGameObjectOnly(worldLoot);
    }

    internal static void DestroyWorldLootGameObjectOnly(LootItem worldLoot){
        if(!worldLoot) return;

        UnregisterWorldLootFromGameWorld(worldLoot);

        try{
            worldLoot.enabled = false;
        }
        catch{
            // ignored
        }

        if(worldLoot.gameObject) Object.Destroy(worldLoot.gameObject);
    }

    private static void UnregisterWorldLootFromGameWorld(LootItem worldLoot){
        if(!worldLoot || !Singleton<GameWorld>.Instantiated) return;

        try{
            var lootList = Singleton<GameWorld>.Instance.LootList;

            lootList?.Remove(worldLoot);
        }
        catch(Exception ex){
            LootInVicinityPlugin.Log?.LogDebug(
                                               PluginInfo.Format(
                                                                 $"Could not remove world loot from LootList: {ex.Message}"
                                                                )
                                              );
        }
    }

    private static void TrySyncCurrentAddressAfterTake(Item item, ItemAddress destination){
        if(item == null) return;

        var controller = VicinityLocalPlayer.InventoryController;

        if(controller == null) return;

        if(item.CurrentAddress?.GetOwnerOrNull() == controller) return;

        var target = destination;

        if(target == null || VicinityStashGrid.IsVicinityStashAddress(target)) target = item.CurrentAddress;

        if(target == null || VicinityStashGrid.IsVicinityStashAddress(target)){
            if(!VicinityPlayerInventory.IsInLocalPlayerInventory(item)) return;

            target = item.CurrentAddress;

            if(target == null || VicinityStashGrid.IsVicinityStashAddress(target)) return;
        }

        if(target.GetOwnerOrNull() != controller) return;

        try{
            if(!target.Equals(item.CurrentAddress)) item.CurrentAddress = target;

            item.RaiseRefreshEvent();

            LootInVicinityPlugin.Log?.LogDebug(
                                               PluginInfo.Format(
                                                                 $"Repaired CurrentAddress after vicinity take for {item.TemplateId}."
                                                                )
                                              );
        }
        catch(Exception ex){
            LootInVicinityPlugin.Log?.LogWarning(PluginInfo.Format($"Could not sync address after take: {ex.Message}"));
        }
    }
}
