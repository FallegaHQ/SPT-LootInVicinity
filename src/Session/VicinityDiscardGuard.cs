using EFT.InventoryLogic;

namespace Softwyx.LootInVicinity.Session;

internal static class VicinityDiscardGuard{
    public static bool ShouldBlockDiscard(Item item){
        if(item == null || !VicinityPanelPresenter.IsPanelVisible) return false;

        var grid = VicinityLootSession.GetVicinityGrid();

        return grid != null && grid.Contains(item);
    }
}
