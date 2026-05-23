using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using System.Collections.Generic;
using System.Linq;

namespace Softwyx.LootInVicinity.Session;

/// <summary>Maps listed panel rows to world <see cref="LootItem"/> piles and world-owner subscriptions.</summary>
internal static class VicinityListedLootRegistry{
    private static readonly Dictionary<string, LootItem> ListedToWorld = new();

    public static int Count => ListedToWorld.Count;

    public static void Clear(){
        ListedToWorld.Clear();
    }

    public static bool HasBinding(Item item){
        return item != null && ListedToWorld.ContainsKey(item.Id);
    }

    public static bool TryGetWorldLoot(Item item, out LootItem worldLoot){
        worldLoot = null;

        return item != null && ListedToWorld.TryGetValue(item.Id, out worldLoot);
    }

    public static bool TryGetWorldLoot(string itemId, out LootItem worldLoot){
        worldLoot = null;

        return !string.IsNullOrEmpty(itemId) && ListedToWorld.TryGetValue(itemId, out worldLoot);
    }

    public static void Register(Item item, LootItem worldLoot){
        ListedToWorld[item.Id] = worldLoot;
    }

    public static bool Remove(string itemId){
        return !string.IsNullOrEmpty(itemId) && ListedToWorld.Remove(itemId);
    }

    public static bool ItemStillOnWorldLootAfterRemove(Item item, ItemAddress destination){
        if(item == null || destination == null) return false;

        if(!ListedToWorld.TryGetValue(item.Id, out var worldLoot) || worldLoot?.ItemOwner == null) return false;

        return destination.GetOwnerOrNull() == worldLoot.ItemOwner;
    }

    public static bool OtherListedItemsShareWorldLoot(LootItem worldLoot, string takenItemId){
        foreach(var pair in ListedToWorld){
            if(pair.Value != worldLoot) continue;

            if(pair.Key != takenItemId) return true;
        }

        return false;
    }

    public static void UnsubscribeAllWorldOwners(){
        foreach(var worldLoot in ListedToWorld.Values.ToList()) UnsubscribeWorldLoot(worldLoot);
    }

    public static void SubscribeWorldOwner(LootItem worldLoot){
        if(worldLoot?.ItemOwner == null) return;

        VicinityItemOwnerEvents.RemoveRemoveHandler(worldLoot.ItemOwner, VicinityWorldOwnerRemoveHandler.OnRemoveItem);
        VicinityItemOwnerEvents.AddRemoveHandler(worldLoot.ItemOwner, VicinityWorldOwnerRemoveHandler.OnRemoveItem);
    }

    public static void UnsubscribeWorldOwner(Item item){
        if(item == null) return;

        if(!ListedToWorld.TryGetValue(item.Id, out var worldLoot)) return;

        UnsubscribeWorldLoot(worldLoot);
    }

    public static void UnsubscribeWorldLoot(LootItem worldLoot){
        if(worldLoot?.ItemOwner == null) return;

        VicinityItemOwnerEvents.RemoveRemoveHandler(worldLoot.ItemOwner, VicinityWorldOwnerRemoveHandler.OnRemoveItem);
    }
}
