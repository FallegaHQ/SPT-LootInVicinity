using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

namespace Softwyx.LootInVicinity.LivPlayer;

internal static class VicinityLocalPlayer{
    private static Player              _raidPlayer;
    private static InventoryController _cachedController;

    public static Player Find(){
        TryBind();

        return _raidPlayer;
    }

    public static Inventory Inventory => InventoryController?.Inventory;

    public static InventoryController InventoryController{
        get{
            TryBind();

            return _cachedController;
        }
    }

    public static bool MatchesInventoryController(InventoryController inventoryController){
        if(inventoryController == null) return false;

        return TryBind() && _cachedController == inventoryController;
    }

    public static bool TryBind(){
        if(_raidPlayer && _cachedController != null) return true;

        var player = ResolveLocalPlayer();

        if(!player) return false;

        _raidPlayer       = player;
        _cachedController = player.InventoryController;

        return true;
    }

    public static void Clear(){
        _raidPlayer       = null;
        _cachedController = null;
    }

    private static Player ResolveLocalPlayer(){
        var ownerPlayer = GamePlayerOwner.MyPlayer;

        if(IsLocalHumanPlayer(ownerPlayer)) return ownerPlayer;

        if(!Singleton<GameWorld>.Instantiated) return null;

        var mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;

        return IsLocalHumanPlayer(mainPlayer) ? mainPlayer : null;
    }

    private static bool IsLocalHumanPlayer(Player player){
        if(!player) return false;

        if(player.IsYourPlayer) return true;

        return !player.IsAI && player.HasGamePlayerOwner;
    }
}
