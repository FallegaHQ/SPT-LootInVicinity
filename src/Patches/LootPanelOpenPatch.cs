using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Postfix on <see cref="ItemsPanel.Show"/> --
/// delegates to <see cref="VicinityItemsPanelOpenHandler"/>.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class LootPanelOpenPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        var method = AccessTools.Method(typeof(ItemsPanel), nameof(ItemsPanel.Show));

        if(method != null) return method;

        method = AccessTools.GetDeclaredMethods(typeof(ItemsPanel)).
                             FirstOrDefault(
                                            m => m.Name       == nameof(ItemsPanel.Show)
                                              && m.ReturnType == typeof(Task)
                                              && m.GetParameters().
                                                   Length
                                              >= 15
                                           );

        if(method == null)
            LootInVicinityPlugin.Log?.LogError(PluginInfo.Format("Harmony target ItemsPanel.Show was not found."));

        return method;
    }

    [PatchPostfix]
    public static void PatchPostfix(
        ItemsPanel          __instance, Task __result, ItemContextAbstractClass sourceContext, CompoundItem lootItem,
        InventoryController inventoryController, ItemsPanel.EItemsTab currentTab
    ){
        VicinityItemsPanelOpenHandler.OnItemsPanelShow(
                                                       __instance,
                                                       __result,
                                                       sourceContext,
                                                       lootItem,
                                                       inventoryController,
                                                       currentTab
                                                      );
    }
}
