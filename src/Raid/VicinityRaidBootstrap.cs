using Comfort.Common;
using EFT;
using Softwyx.LootInVicinity.LivPlayer;

namespace Softwyx.LootInVicinity.Raid;

internal static class VicinityRaidBootstrap{
    public static bool IsReady => VicinityRaidServices.IsReady;

    public static bool IsRegisteredInWorld{
        get;
        private set;
    }

    public static bool EnsureRaidStash(){
        if(!Settings.Enabled.Value || !VicinityLifecycle.RaidSessionActive) return false;

        if(IsReady) return true;

        if(!Singleton<GameWorld>.Instantiated || !VicinityLocalPlayer.TryBind()) return false;

        var stash = Singleton<ItemFactoryClass>.Instance.CreateFakeStash();
        var grid  = new VicinityStashGrid("vicinityGrid", stash);

        stash.Grids = [grid];

        VicinityRaidServices.RadiusStash = stash;
        VicinityRaidServices.VicinityTrader = new TraderControllerClass(
                                                                        stash,
                                                                        VicinityLootSession.OwnerId,
                                                                        Settings.FormatPanelTitle(0),
                                                                        false
                                                                       );

        return IsReady;
    }

    public static bool RegisterInWorld(){
        if(!EnsureRaidStash()) return false;

        if(IsRegisteredInWorld) return true;

        VicinityLootSession.BindTraderEvents(VicinityRaidServices.VicinityTrader);
        IsRegisteredInWorld = true;

        return true;
    }

    public static void UnregisterFromWorld(){
        if(!IsRegisteredInWorld) return;

        VicinityLootSession.UnbindTraderEvents(VicinityRaidServices.VicinityTrader);
        IsRegisteredInWorld = false;
    }

    public static void Release(){
        UnregisterFromWorld();

        if(VicinityRaidServices.RadiusStash != null) VicinityLootSession.ClearStashContents();

        VicinityLootSession.ResetRaidState();

        VicinityRaidServices.Clear();
    }
}
