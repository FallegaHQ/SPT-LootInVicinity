using System;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Softwyx.LootInVicinity.Config;
using Softwyx.LootInVicinity.Grid;
using Softwyx.LootInVicinity.LivPlayer;
using Softwyx.LootInVicinity.Raid;
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

        if(!Singleton<IGameLevel>.Instantiated) return;

        var world = Singleton<IGameLevel>.Instance;

        foreach(var item in grid.Items.ToList()){
            if(!VicinityStagingRegistry.IsStaged(item)) continue;

            TryDropAtFeet(item, player, trader, world, grid);
            VicinityStagingRegistry.Unregister(item);
        }

        VicinityStagingRegistry.Clear();
    }

    private static void TryDropAtFeet(
        Item item, object player, ItemController trader, IGameLevel world, VicinityStashGrid grid
    ){
        try{
            if(grid.Contains(item)){
                var remove = ItemManipulator.Remove(item, trader);

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
                LootInVicinityPlugin.Log?.LogWarning(PluginInfo.Format("ThrowItem not found on EFT.IGameLevel."));

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

        _throwItemMethod = typeof(IGameLevel).GetMethod("ThrowItem", [typeof(Item), iPlayerType, typeof(Vector3?)]);

        return _throwItemMethod;
    }
}
