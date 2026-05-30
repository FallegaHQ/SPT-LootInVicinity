using Comfort.Common;
using EFT.InventoryLogic;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Softwyx.LootInVicinity.Session;

internal static class VicinityStagingDrop{
    private static MethodInfo _throwItemMethod;

    public static void DropAllBeforeClear(){
        if(!Settings.AllowVicinityStaging.Value) return;

        var player = VicinityLocalPlayer.Find();
        var grid   = VicinityLootSession.GetVicinityGrid();
        var trader = VicinityRaidServices.VicinityTrader;

        if(!player || grid == null || trader == null) return;

        if(!Singleton<GInterface169>.Instantiated) return;

        var world = Singleton<GInterface169>.Instance;

        foreach(var item in grid.Items.ToList()){
            if(!VicinityStagingRegistry.IsStaged(item)) continue;

            TryDropAtFeet(item, player, trader, world, grid);
            VicinityStagingRegistry.Unregister(item);
        }

        VicinityStagingRegistry.Clear();
    }

    private static void TryDropAtFeet(
        Item item, object player, TraderControllerClass trader, GInterface169 world, VicinityStashGrid grid
    ){
        try{
            if(grid.Contains(item)){
                var remove = InteractionsHandlerClass.Remove(item, trader, false);

                if(remove.Failed){
                    LootInVicinityPlugin.Log?.LogWarning(
                                                         PluginInfo.Format(
                                                                           $"Could not remove staged item from vicinity stash before drop: {remove.Error}"
                                                                          )
                                                        );
                    grid.ForceRemoveListedItem(item);
                }
            }

            var throwMethod = ResolveThrowItemMethod(player);

            if(throwMethod == null){
                LootInVicinityPlugin.Log?.LogWarning(PluginInfo.Format("ThrowItem not found on GInterface169."));

                return;
            }

            throwMethod.Invoke(world, [item, player, (Vector3?) Vector3.down]);
        }
        catch(Exception ex){
            LootInVicinityPlugin.Log?.LogWarning(
                                                 PluginInfo.Format(
                                                                   $"Failed to drop staged item {item?.TemplateId}: {ex.Message}"
                                                                  )
                                                );
        }
    }

    private static MethodInfo ResolveThrowItemMethod(object player){
        if(_throwItemMethod != null) return _throwItemMethod;

        var iPlayerType = player?.GetType().
                                  GetInterfaces().
                                  FirstOrDefault(i => i.Name == "IPlayer");

        if(iPlayerType == null) return null;

        _throwItemMethod = typeof(GInterface169).GetMethod("ThrowItem", [typeof(Item), iPlayerType, typeof(Vector3?)]);

        return _throwItemMethod;
    }
}
