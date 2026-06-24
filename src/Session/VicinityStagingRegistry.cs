using System.Collections.Generic;
using EFT.InventoryLogic;

namespace Softwyx.LootInVicinity.Session;

internal static class VicinityStagingRegistry{
    private static readonly HashSet<string> StagedItemIds = new();

    public static void Register(Item item){
        if(item == null) return;

        StagedItemIds.Add(item.Id);
    }

    public static void Unregister(Item item){
        if(item == null) return;

        StagedItemIds.Remove(item.Id);
    }

    public static bool IsStaged(Item item){
        return item != null && StagedItemIds.Contains(item.Id);
    }

    public static void Clear(){
        StagedItemIds.Clear();
    }
}
