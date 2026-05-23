using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Prefix on <see cref="InventoryScreen.Close"/> --
/// delegates to <see cref="VicinityInventoryCloseHandler"/>.</summary>
internal sealed class InventoryScreenClosePatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(typeof(InventoryScreen), nameof(InventoryScreen.Close));
    }

    [PatchPrefix]
    public static void PatchPrefix(){
        VicinityInventoryCloseHandler.OnInventoryScreenClose();
    }
}
