using Comfort.Common;
using EFT;
using EFT.Interactive;

namespace Softwyx.LootInVicinity.World;

/// <summary>DestroyLoot prefix -- skip vanilla destroy for vicinity-listed loot and in-inventory pickups.</summary>
internal static class VicinityDestroyLootHandler{
    /// <param name="loot"></param>
    /// <returns>Whether vanilla <see cref="GameWorld.DestroyLoot(IKillableLootItem)"/> should run.</returns>
    public static bool ShouldRunVanillaDestroyLoot(IKillableLootItem loot){
        if(loot is not LootItem worldLoot) return true;

        if(!Settings.Enabled.Value || !Singleton<GameWorld>.Instantiated) return true;

        var item = worldLoot.Item;

        if(item == null) return true;

        if(VicinityLootSession.HasListedWorldBinding(item)
        || VicinityListedLootRegistry.IsRegisteredWorldLoot(worldLoot)
        || VicinityTakeCleanup.IsPendingTake(item.Id)){
            LootInVicinityPlugin.Log?.LogDebug(
                                               PluginInfo.Format(
                                                                 $"DestroyLoot skipped for vicinity-listed item "
                                                               + $"{item.TemplateId} (deferred cleanup)."
                                                                )
                                              );

            return false;
        }

        if(!VicinityPlayerInventory.IsInLocalPlayerInventory(item)) return true;

        VicinityLootSession.DestroyWorldLootGameObjectOnly(worldLoot);

        LootInVicinityPlugin.Log?.LogDebug(
                                           PluginInfo.Format(
                                                             $"DestroyLoot skipped for in-inventory item "
                                                           + $"{item.TemplateId} (world GO only)."
                                                            )
                                          );

        return false;
    }
}
