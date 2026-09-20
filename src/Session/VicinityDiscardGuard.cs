using EFT.InventoryLogic;
using Softwyx.LootInVicinity.Config;
using Softwyx.LootInVicinity.Raid;
using Softwyx.LootInVicinity.Ui;
using Softwyx.LootInVicinity.World;

namespace Softwyx.LootInVicinity.Session;

internal static class VicinityDiscardGuard{
    public static bool ShouldBlockDiscard(Item item){
        if(item == null || !Settings.Enabled.Value || !VicinityLifecycle.RaidSessionActive) return false;

        if(!VicinityPanelPresenter.IsPanelVisible) return false;

        // Consumed meds/food call Discard via EFT.HealthSystem.MedEffectHelper.RemoveItem; allow removal when depleted.
        if(VicinityListedWorldCleanup.IsWorldRepresentationObsolete(item)) return false;

        var grid = VicinityLootSession.GetVicinityGrid();

        return grid != null && grid.Contains(item);
    }
}
