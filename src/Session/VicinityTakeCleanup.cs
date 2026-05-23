using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using Softwyx.LootInVicinity.LivPlayer;

namespace Softwyx.LootInVicinity.Session;

/// <summary>Take completion, address sync, and deferred world-pile destroy after panel pickup.</summary>
internal static class VicinityTakeCleanup{
    private static readonly HashSet<string> PendingTakeIds = [];

    public static void ScheduleTakeFromPanel(Item item, ItemAddress destinationAfterTake = null){
        if(item == null) return;

        VicinityListedLootRegistry.TryGetWorldLoot(item, out var worldLoot);

        var grid = VicinityLootSession.GetVicinityGrid();

        if(!VicinityListedLootRegistry.HasBinding(item) && (grid == null || !grid.Contains(item))) return;

        CompleteTakeFromPanel(item, destinationAfterTake);

        if(!worldLoot || LootInVicinityPlugin.Instance == null) return;

        if(!PendingTakeIds.Add(item.Id)) return;

        LootInVicinityPlugin.Instance.StartCoroutine(DeferredDestroyWorldLootRoutine(worldLoot, item));
    }

    public static void CompleteTakeFromPanel(Item item, ItemAddress destinationAfterTake = null){
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

    private static IEnumerator DeferredDestroyWorldLootRoutine(LootItem worldLoot, Item takenItem){
        yield return null;

        var itemId = takenItem?.Id;

        try{
            if(VicinityLootSession.SuppressWorldSync || VicinityLootSession.IsPopulating || !worldLoot) yield break;

            if(!Singleton<GameWorld>.Instantiated) yield break;

            if(takenItem != null && !VicinityPlayerInventory.IsInLocalPlayerInventory(takenItem)) yield break;

            if(VicinityListedLootRegistry.OtherListedItemsShareWorldLoot(worldLoot, itemId)) yield break;

            DestroyWorldLootRepresentation(worldLoot, takenItem);
        }
        finally{
            if(!string.IsNullOrEmpty(itemId)) PendingTakeIds.Remove(itemId);
        }
    }

    internal static void DestroyWorldLootRepresentation(LootItem worldLoot, Item takenItem){
        if(!worldLoot) return;

        if(takenItem != null && worldLoot.Item != null && ReferenceEquals(worldLoot.Item, takenItem)){
            DestroyWorldLootGameObjectOnly(worldLoot);

            return;
        }

        if(!Singleton<GameWorld>.Instantiated) return;

        try{
            Singleton<GameWorld>.Instance.DestroyLoot(worldLoot);
        }
        catch(Exception ex){
            LootInVicinityPlugin.Log?.LogWarning(
                                                 PluginInfo.Format(
                                                                   $"DestroyLoot failed for {worldLoot.Item?.TemplateId}: {ex}"
                                                                  )
                                                );

            DestroyWorldLootGameObjectOnly(worldLoot);
        }
    }

    internal static void DestroyWorldLootGameObjectOnly(LootItem worldLoot){
        if(!worldLoot) return;

        try{
            worldLoot.enabled = false;
        }
        catch{
            // ignored
        }

        if(worldLoot.gameObject) UnityEngine.Object.Destroy(worldLoot.gameObject);
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

            item.RaiseRefreshEvent(false, true);

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
