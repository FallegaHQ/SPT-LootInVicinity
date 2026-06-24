namespace Softwyx.LootInVicinity.Raid;

internal static class VicinityLifecycle{
    public static bool RaidSessionActive{
        get;
        private set;
    }

    public static bool PanelTakeCleanupActive =>
        Settings.Enabled.Value
     && RaidSessionActive
     && VicinityPanelPresenter.IsPanelVisible
     && VicinityRaidBootstrap.IsRegisteredInWorld
     && !UiAccess.IsWorldContainerLootOpen()
     && !VicinityPanelPresenter.IsInventoryClosing;

    public static bool QuestRoutingActive =>
        Settings.Enabled.Value && Settings.RouteQuestItemsToTaskStash.Value && PanelTakeCleanupActive;

    public static void OnRaidStarted(){
        RaidSessionActive = true;
        VicinityLocalPlayer.Clear();
        VicinityRaidBootstrap.Release();
        VicinityPanelPresenter.ResetPanelState();
        LootInVicinityPlugin.ScheduleRaidBootstrap();

        LootInVicinityPlugin.Log?.LogInfo(PluginInfo.Format("Raid started."));
    }

    public static void OnRaidEnded(){
        RaidSessionActive = false;
        VicinityLocalPlayer.Clear();
        VicinityPanelPresenter.HideIfActive(null);
        VicinityRaidBootstrap.Release();
        VicinityPanelPresenter.ResetPanelState();
    }
}
