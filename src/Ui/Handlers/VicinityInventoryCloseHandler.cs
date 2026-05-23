namespace Softwyx.LootInVicinity.Ui.Handlers;

/// <summary>Inventory screen close -- hide vicinity panel before vanilla close finishes.</summary>
internal static class VicinityInventoryCloseHandler{
    public static void OnInventoryScreenClose(){
        if(!Settings.Enabled.Value) return;

        if(!VicinityPanelPresenter.HasActiveVicinityWork && VicinityLootSession.ListedCount == 0) return;

        VicinityPanelPresenter.HideIfActive(null);
    }
}
