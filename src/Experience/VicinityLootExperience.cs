using EFT.InventoryLogic;
using System;
using Softwyx.LootInVicinity.LivPlayer;

namespace Softwyx.LootInVicinity.Experience;

/// <summary>
/// Awards raid XP/skills when items are taken via the vicinity panel.
/// </summary>
internal static class VicinityLootExperience{
    public static void TryGrantForTake(Item item){
        if(item == null || !VicinityLifecycle.RaidSessionActive) return;

        if(!VicinityPlayerInventory.IsInLocalPlayerInventory(item)) return;

        var player = VicinityLocalPlayer.Find();

        if(player?.StatisticsManager == null) return;

        try{
            player.StatisticsManager.OnGrabLoot(item);
        }
        catch(Exception ex){
            LootInVicinityPlugin.Log?.LogWarning(PluginInfo.Format($"Could not grant loot XP: {ex.Message}"));
        }
    }
}
