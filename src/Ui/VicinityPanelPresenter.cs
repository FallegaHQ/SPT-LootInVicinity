using System;
using System.Collections;
using System.Collections.Generic;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using UnityEngine;

namespace Softwyx.LootInVicinity.Ui;

internal static class VicinityPanelPresenter{
    private static MonoBehaviour _attachHost;
    private static Coroutine     _attachCoroutine;

    public static bool IsPanelVisible{
        get;
        private set;
    }

    private static bool IsAttachInProgress{
        get;
        set;
    }

    private static ItemsPanel BoundItemsPanel{
        get;
        set;
    }

    public static bool HasActiveVicinityWork => IsPanelVisible || IsAttachInProgress;

    public static bool IsInventoryClosing{
        get;
        private set;
    }

    private static bool EnsureFieldsResolved(){
        if(VicinityUiReflection.EnsureItemsPanelFields()) return true;

        LootInVicinityPlugin.Log?.LogError(
                                           PluginInfo.Format("Could not resolve ItemsPanel simple stash panel ")
                                         + "field for UI attach."
                                          );

        return false;
    }

    public static bool TryGetSimpleStashPanel(ItemsPanel itemsPanel, out SimpleStashPanel simpleStashPanel){
        simpleStashPanel = VicinityUiReflection.GetSimpleStashPanel(itemsPanel);

        return simpleStashPanel;
    }

    public static void ResetPanelState(){
        CancelAttachRoutine();
        BoundItemsPanel    = null;
        IsPanelVisible     = false;
        IsAttachInProgress = false;
        IsInventoryClosing = false;
        UiAccess.ResetRaidUiState();
    }

    public static void CancelAttachRoutine(){
        if(_attachHost && _attachCoroutine != null) _attachHost.StopCoroutine(_attachCoroutine);

        ClearAttachState();
    }

    public static void BeginAttachFromItemsPanel(ItemsPanel itemsPanel, IEnumerator attachRoutine){
        CancelAttachRoutine();

        _attachHost      = itemsPanel;
        _attachCoroutine = itemsPanel.StartCoroutine(attachRoutine);
    }

    /// <summary>
    ///     Scans nearby loot, fills <see cref="VicinityRaidServices.RadiusStash" />, and shows it on
    ///     <paramref name="simpleStashPanel" />. Started from
    ///     <see cref="Softwyx.LootInVicinity.Patches.LootPanelOpenPatch" />.
    /// </summary>
    /// <param name="itemsPanel">Raid tab <see cref="ItemsPanel" /> receiving the vicinity stash.</param>
    /// <param name="simpleStashPanel">
    ///     Right-hand <see cref="SimpleStashPanel" /> to call
    ///     <see cref="SimpleStashPanel.Show" /> on.
    /// </param>
    /// <param name="inventoryController">Local player inventory for the panel.</param>
    /// <param name="sourceContext">Parent item context; used to build the stash context child.</param>
    /// <param name="currentTab">Active <see cref="ItemsPanel.EItemsTab" /> passed to Show.</param>
    /// <param name="uiDisposableList">Optional disposables list; panel is registered when non-null.</param>
    /// <returns>Yields while scan, stash populate, and panel show run.</returns>
    public static IEnumerator AttachNearbyPanelRoutine(
        ItemsPanel               itemsPanel, SimpleStashPanel simpleStashPanel, InventoryController inventoryController,
        ItemContextAbstractClass sourceContext, ItemsPanel.EItemsTab currentTab, AddViewListClass uiDisposableList
    ){
        if(IsAttachInProgress) yield break;

        if(!VicinityLifecycle.RaidSessionActive) yield break;

        if(!VicinityLocalPlayer.MatchesInventoryController(inventoryController)) yield break;

        if(HasActiveVicinityWork){
            UiAccess.SetWorldLootOpen(false);
            TearDown(BoundItemsPanel ?? itemsPanel);
        }

        yield return null;

        if(!UiAccess.CanAttachVicinityPanel(itemsPanel, null, inventoryController)) yield break;

        IsAttachInProgress = true;

        var candidates = new List<LootItem>();
        var seen       = new HashSet<string>();

        yield return VicinityLootScanner.CollectCandidatesRoutine(candidates, seen);

        var maxItems = Settings.MaxItemsInPanel.Value;

        if(maxItems > 0 && candidates.Count > maxItems) candidates.RemoveRange(maxItems, candidates.Count - maxItems);

        if((candidates.Count == 0 && Settings.HidePanelWhenEmpty.Value && !Settings.AllowVicinityStaging.Value)
        || !UiAccess.CanAttachVicinityPanel(itemsPanel, null, inventoryController)){
            ClearAttachState();
        }
        else{
            var stash = VicinityRaidServices.RadiusStash;

            if(stash == null){
                ClearAttachState();

                yield break;
            }

            var failed = false;

            VicinityLootSession.SetPopulating(true);

            try{
                yield return VicinityLootSession.PlaceCandidatesRoutine(candidates, _ => {});

                var showPanel = VicinityLootSession.ListedCount > 0
                             || !Settings.HidePanelWhenEmpty.Value
                             || Settings.AllowVicinityStaging.Value;

                if(!showPanel){
                    ClearAttachState();

                    yield break;
                }

                if(!VicinityRaidBootstrap.RegisterInWorld()){
                    VicinityLootSession.ClearStashContents();
                    ClearAttachState();

                    yield break;
                }

                yield return null;

                try{
                    var stashContext = VicinityStashItemContext.Create(sourceContext, stash, simpleStashPanel);

                    if(stashContext == null){
                        ClearAttachState();

                        yield break;
                    }

                    simpleStashPanel.Show(
                                          stash,
                                          inventoryController,
                                          stashContext,
                                          true,
                                          null,
                                          SimpleStashPanel.EStashSearchAvailability.All,
                                          inventoryController,
                                          currentTab
                                         );
                    uiDisposableList?.AddDisposable(simpleStashPanel);

                    VicinityUiReflection.SetRightPaneCompoundItem(stash);

                    VicinityUiReflection.ApplyPanelTitle(
                                                         simpleStashPanel,
                                                         Settings.FormatPanelTitle(VicinityLootSession.ListedCount)
                                                        );
                }
                catch(Exception ex){
                    LootInVicinityPlugin.Log?.LogError(
                                                       PluginInfo.Format("Failed to show vicinity ")
                                                     + $"panel: {ex.Message}"
                                                      );
                    VicinityLootSession.ClearStashContents();
                    VicinityRaidBootstrap.UnregisterFromWorld();
                    failed = true;
                }
            }
            finally{
                VicinityLootSession.SetPopulating(false);
            }

            if(failed){
                ClearAttachState();

                yield break;
            }

            BoundItemsPanel = itemsPanel;
            IsPanelVisible  = true;

            yield return null;

            if(!IsPanelVisible) yield break;

            VicinityUiReflection.BindGridViews(
                                               VicinityLootSession.GetVicinityGrid(),
                                               GetSimpleStashPanel(BoundItemsPanel)
                                              );

            ClearAttachState();
        }
    }

    public static void HideIfActive(ItemsPanel itemsPanel){
        if(!IsPanelVisible && !IsAttachInProgress) return;

        if(itemsPanel != null && BoundItemsPanel != null && itemsPanel != BoundItemsPanel) return;

        UiAccess.SetWorldLootOpen(false);
        TearDown(itemsPanel ?? BoundItemsPanel);
    }

    private static SimpleStashPanel GetSimpleStashPanel(ItemsPanel itemsPanel){
        if(!UiAccess.IsRaidItemsPanel(itemsPanel) || !EnsureFieldsResolved()) return null;

        return VicinityUiReflection.GetSimpleStashPanel(itemsPanel);
    }

    private static void TearDown(ItemsPanel itemsPanel){
        IsInventoryClosing = true;

        try{
            VicinityRaidBootstrap.UnregisterFromWorld();
            VicinityLootSession.ClearStashContents();
            UiAccess.ClearDragLayer();

            if(itemsPanel){
                var simpleStashPanel = GetSimpleStashPanel(itemsPanel);

                if(simpleStashPanel)
                    try{
                        simpleStashPanel.Close();
                    }
                    catch{
                        // ignored
                    }
            }

            VicinityUiReflection.ClearRightPaneCompoundItem(VicinityRaidServices.RadiusStash);
        }
        finally{
            IsPanelVisible     = false;
            BoundItemsPanel    = null;
            IsInventoryClosing = false;
        }
    }

    private static void ClearAttachState(){
        _attachCoroutine   = null;
        _attachHost        = null;
        IsAttachInProgress = false;
    }
}
