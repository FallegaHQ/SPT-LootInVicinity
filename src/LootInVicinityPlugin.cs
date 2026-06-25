using System;
using System.Collections;
using BepInEx;
using BepInEx.Logging;
using Softwyx.LootInVicinity.Patches;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.SPT.core", "4.0.0")]
public class LootInVicinityPlugin : BaseUnityPlugin{
    internal static ManualLogSource      Log;
    internal static LootInVicinityPlugin Instance;

    private void Awake(){
        Instance = this;
        Log      = Logger;

        if(!PreloadDefaultLocale()) return;

        Settings.Init(Config);

        EnablePatch<LocaleApplicationLanguagePatch>("LocaleApplicationLanguagePatch");
        EnablePatch<RaidStartedPatch>("RaidStartedPatch");
        EnablePatch<RaidEndPatch>("RaidEndPatch");
        EnablePatch<LootPanelOpenPatch>("LootPanelOpenPatch");
        EnablePatch<InventoryScreenClosePatch>("InventoryScreenClosePatch");
        EnablePatch<DestroyLootPatch>("DestroyLootPatch");
        EnablePatch<BotItemTakerRefreshPatch>("BotItemTakerRefreshPatch");
        EnablePatch<VicinityListedQuickFindFlagsPatch>("VicinityListedQuickFindFlagsPatch");
        EnablePatch<VicinityItemUiListedQuickFindPatch>("VicinityItemUiListedQuickFindPatch");
        EnablePatch<VicinityInteractionsMovePatch>("VicinityInteractionsMovePatch");
        EnablePatch<VicinityQuickFindPatch>("VicinityQuickFindPatch");
        EnablePatch<VicinityItemUiQuickFindPatch>("VicinityItemUiQuickFindPatch");
        EnablePatch<QuestItemMovePatch>("QuestItemMovePatch");
        EnablePatch<VicinityDiscardPatch>("VicinityDiscardPatch");
        EnablePatch<VicinityDiscardResultPatch>("VicinityDiscardResultPatch");
        EnablePatch<VicinityItemUseCleanupPatch>("VicinityItemUseCleanupPatch");
        EnablePatch<VicinityItemUseAllCleanupPatch>("VicinityItemUseAllCleanupPatch");
        EnablePatch<VicinityRemoveItemPatch>("VicinityRemoveItemPatch");

        Log.LogInfo(PluginInfo.Format($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} loaded."));
    }

    private static bool PreloadDefaultLocale(){
        if(LocaleLoader.PreloadDefaultLocale(out var error)) return true;

        Log.LogError(PluginInfo.Format($"Locale preload failed: {error} Mod patches were not enabled."));

        return false;
    }

    private static void EnablePatch<T>(string name) where T : ModulePatch, new(){
        try{
            new T().Enable();
            Log.LogInfo(PluginInfo.Format($"{name} enabled."));
        }
        catch(Exception ex){
            Log.LogError(PluginInfo.Format($"{name} failed: {ex}"));
        }
    }

    internal static void ScheduleRaidBootstrap(){
        if(Instance == null) return;

        Instance.StartCoroutine(RaidBootstrapRoutine());
    }

    private static IEnumerator RaidBootstrapRoutine(){
        for(var frame = 0; frame < 180; frame++){
            if(!VicinityLifecycle.RaidSessionActive) yield break;

            if(VicinityLocalPlayer.TryBind() && VicinityRaidBootstrap.EnsureRaidStash()) yield break;

            yield return null;
        }
    }
}
