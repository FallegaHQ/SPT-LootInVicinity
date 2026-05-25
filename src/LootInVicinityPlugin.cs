using BepInEx;
using BepInEx.Logging;
using Softwyx.LootInVicinity.Patches;
using SPT.Reflection.Patching;
using System.Collections;
using Softwyx.LootInVicinity.LivPlayer;

namespace Softwyx.LootInVicinity;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.SPT.core", "4.0.0")]
public class LootInVicinityPlugin : BaseUnityPlugin{
    internal static ManualLogSource      Log;
    internal static LootInVicinityPlugin Instance;

    private void Awake(){
        Instance = this;
        Log      = Logger;

        Settings.Init(Config);

        EnablePatch<RaidStartedPatch>("RaidStartedPatch");
        EnablePatch<RaidEndPatch>("RaidEndPatch");
        EnablePatch<LootPanelOpenPatch>("LootPanelOpenPatch");
        EnablePatch<InventoryScreenClosePatch>("InventoryScreenClosePatch");
        EnablePatch<DestroyLootPatch>("DestroyLootPatch");
        EnablePatch<VicinityListedQuickFindFlagsPatch>("VicinityListedQuickFindFlagsPatch");
        EnablePatch<VicinityInteractionsMovePatch>("VicinityInteractionsMovePatch");
        EnablePatch<VicinityQuickFindPatch>("VicinityQuickFindPatch");
        EnablePatch<VicinityItemUiQuickFindPatch>("VicinityItemUiQuickFindPatch");
        EnablePatch<QuestItemMovePatch>("QuestItemMovePatch");

        Log.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} loaded.");
    }

    private static void EnablePatch<T>(string name) where T : ModulePatch, new(){
        try{
            new T().Enable();
            Log.LogInfo(PluginInfo.Format($"{name} enabled."));
        }
        catch(System.Exception ex){
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

internal static class PluginInfo{
    public const  string PLUGIN_GUID    = "com.softwyx.lootinvicinity";
    public const  string PLUGIN_NAME    = "Loot In Vicinity";
    public const  string PLUGIN_VERSION = "2.15.131";
    private const string LOGPrefix      = "LIV";

    public static string Format(string message){
        return $"[{LOGPrefix}] {message}";
    }
}
