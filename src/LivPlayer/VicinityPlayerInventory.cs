using EFT.InventoryLogic;

namespace Softwyx.LootInVicinity.LivPlayer;

internal static class VicinityPlayerInventory{
    public static bool IsInLocalPlayerInventory(Item item){
        if(item?.CurrentAddress == null) return false;

        var controller = VicinityLocalPlayer.InventoryController;

        if(controller == null) return false;

        return item.CurrentAddress.GetOwnerOrNull() == controller;
    }
}
