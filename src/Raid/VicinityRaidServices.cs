using EFT.InventoryLogic;

namespace Softwyx.LootInVicinity.Raid;

/// <summary>Per-raid fake stash and trader controller used by the vicinity panel UI.</summary>
internal static class VicinityRaidServices{
    public static Stash RadiusStash{
        get;
        set;
    }

    public static ItemController VicinityTrader{
        get;
        set;
    }

    public static bool IsReady => RadiusStash != null && VicinityTrader != null;

    public static void Clear(){
        RadiusStash    = null;
        VicinityTrader = null;
    }
}
