using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;

namespace Softwyx.LootInVicinity.Config;

internal static class Settings{
    private const string DonationUrl = "https://github.com/sponsors/FallegaHQ";

    private const string SupportSectionTitle   = "0. Support";
    private const string GeneralSectionTitle   = "1. General";
    private const string InventorySectionTitle = "2. Inventory panel";
    private const string ScanSectionTitle      = "3. Scanning";
    private const string QuestSectionTitle     = "4. Quest items";

    public static ConfigEntry<bool>  Enabled;
    public static ConfigEntry<float> ScanRadius;
    public static ConfigEntry<bool>  MainScanRequireLineOfSight;
    public static ConfigEntry<int>   MaxItemsInPanel;
    public static ConfigEntry<bool>  HidePanelWhenEmpty;
    public static ConfigEntry<bool>  AllowVicinityStaging;
    public static ConfigEntry<bool>  RouteQuestItemsToTaskStash;
    public static ConfigEntry<bool>  ShowQuestItemsInVicinity;

    private static ConfigEntry<string> _panelTitle;
    private static ConfigEntry<bool>   _appendItemCountToTitle;

    private static readonly List<ConfigEntryBase> ConfigEntries = [];

    public static bool ShouldShowVicinityPanel(bool worldLootOpen){
        if(!Enabled.Value) return false;

        return !worldLootOpen;
    }

    public static string FormatPanelTitle(int listedCount){
        var title = _panelTitle?.Value ?? LocaleLoader.Get(LocaleKeys.PanelTitle);

        if(_appendItemCountToTitle?.Value == true && listedCount > 0)
            return LocaleLoader.Get(LocaleKeys.PanelTitleCount, title, listedCount);

        return title;
    }

    public static void Init(ConfigFile config){
        ConfigEntries.Clear();

        ConfigEntries.Add(
                          config.Bind(
                                      SupportSectionTitle,
                                      "Donate",
                                      DonationUrl,
                                      new ConfigDescription(
                                                            "Optional support via GitHub Sponsors.",
                                                            null,
                                                            DonationAttributes()
                                                           )
                                     )
                         );

        ConfigEntries.Add(
                          Enabled = config.Bind(
                                                GeneralSectionTitle,
                                                "Enabled",
                                                true,
                                                new ConfigDescription("Master toggle for the mod.", null, Basic())
                                               )
                         );

        ConfigEntries.Add(
                          _panelTitle = config.Bind(
                                                    InventorySectionTitle,
                                                    "Panel title",
                                                    LocaleLoader.TryGet(LocaleKeys.PanelTitle) ?? "Nearby Items",
                                                    new ConfigDescription(
                                                                          "Title for the right-hand vicinity panel (Tab inventory).",
                                                                          null,
                                                                          Basic()
                                                                         )
                                                   )
                         );

        ConfigEntries.Add(
                          HidePanelWhenEmpty = config.Bind(
                                                           InventorySectionTitle,
                                                           "Hide panel when empty",
                                                           true,
                                                           new ConfigDescription(
                                                                "Leave the right pane empty when no nearby loot is found.",
                                                                null,
                                                                Advanced()
                                                               )
                                                          )
                         );

        ConfigEntries.Add(
                          _appendItemCountToTitle = config.Bind(
                                                                InventorySectionTitle,
                                                                "Append item count to title",
                                                                true,
                                                                new ConfigDescription(
                                                                     "e.g. \"Nearby Items (7)\" after each scan.",
                                                                     null,
                                                                     Advanced()
                                                                    )
                                                               )
                         );

        ConfigEntries.Add(
                          AllowVicinityStaging = config.Bind(
                                                             InventorySectionTitle,
                                                             "Allow staging in vicinity panel",
                                                             true,
                                                             new ConfigDescription(
                                                                  "Drag items from your inventory into the nearby panel to reorganize. Staged items drop at your feet when you close inventory",
                                                                  null,
                                                                  Basic()
                                                                 )
                                                            )
                         );

        ConfigEntries.Add(
                          ScanRadius = config.Bind(
                                                   ScanSectionTitle,
                                                   "Scan radius (m)",
                                                   3f,
                                                   new ConfigDescription(
                                                                         "Main scan radius (minimum 3 m).",
                                                                         new AcceptableValueRange<float>(3f, 6f),
                                                                         BasicFloat()
                                                                        )
                                                  )
                         );

        ConfigEntries.Add(
                          MainScanRequireLineOfSight = config.Bind(
                                                                   ScanSectionTitle,
                                                                   "Main scan requires line of sight",
                                                                   true,
                                                                   new ConfigDescription(
                                                                        "When enabled, the main scan cylinder only lists loot you can see (no walls/obstacles).",
                                                                        null,
                                                                        Advanced()
                                                                       )
                                                                  )
                         );

        ConfigEntries.Add(
                          MaxItemsInPanel = config.Bind(
                                                        ScanSectionTitle,
                                                        "Max items in panel",
                                                        48,
                                                        new ConfigDescription(
                                                                              "Maximum loot rows shown per Tab open.",
                                                                              new AcceptableValueRange<int>(1, 96),
                                                                              ConfigIntegralUi.IntAttributes(true)
                                                                             )
                                                       )
                         );

        ConfigEntries.Add(
                          RouteQuestItemsToTaskStash = config.Bind(
                                                                   QuestSectionTitle,
                                                                   "Route quest items to task stash",
                                                                   true,
                                                                   new ConfigDescription(
                                                                        "Dragging quest loot from the vicinity panel into your inventory sends it to the quest raid stash.",
                                                                        null,
                                                                        Basic()
                                                                       )
                                                                  )
                         );

        ConfigEntries.Add(
                          ShowQuestItemsInVicinity = config.Bind(
                                                                 QuestSectionTitle,
                                                                 "Show quest items in vicinity",
                                                                 true,
                                                                 new ConfigDescription(
                                                                      "When disabled, quest items are omitted from the nearby loot scan and panel.",
                                                                      null,
                                                                      Basic()
                                                                     )
                                                                )
                         );

        RecalcOrder();
        ConfigFloatUi.SnapEntries(ConfigEntries, step: ScanRadiusStep);
    }

    private const float ScanRadiusStep = 0.05f;

    private static ConfigurationManagerAttributes Basic(){
        return new ConfigurationManagerAttributes{
                                                     IsAdvanced = false
                                                 };
    }

    private static ConfigurationManagerAttributes Advanced(){
        return new ConfigurationManagerAttributes{
                                                     IsAdvanced = true
                                                 };
    }

    private static ConfigurationManagerAttributes BasicFloat(){
        return ConfigFloatUi.Attributes(false, step: ScanRadiusStep);
    }

    private static ConfigurationManagerAttributes DonationAttributes(){
        return new ConfigurationManagerAttributes{
                                                     IsAdvanced        = false,
                                                     ReadOnly          = true,
                                                     HideDefaultButton = true,
                                                     Browsable         = true,
                                                     CustomDrawer      = DrawDonationLink
                                                 };
    }

    private static void DrawDonationLink(ConfigEntryBase _){
        GUILayout.Label("");
        if(GUILayout.Button("GitHub Sponsors")) Application.OpenURL(DonationUrl);
    }

    private static void RecalcOrder(){
        var settingOrder = ConfigEntries.Count;

        foreach(var entry in ConfigEntries){
            if(entry.Description.Tags[0] is ConfigurationManagerAttributes attributes) attributes.Order = settingOrder;

            settingOrder--;
        }
    }
}
