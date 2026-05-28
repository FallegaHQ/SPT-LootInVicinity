using System.Collections;
using System.Collections.Generic;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using UnityEngine;

namespace Softwyx.LootInVicinity.Loot;

internal static class VicinityLootScanner{
    private const float MinMainScanRadius = 3f;
    private const float InnerScanRadius   = 1f;
    private const int   MaxColliderBuffer = 512;

    private static readonly Collider[]     ColliderBuffer = new Collider[MaxColliderBuffer];
    private static readonly List<LootItem> PassResults    = new(MaxColliderBuffer);

    /// <summary>
    /// Fills <paramref name="candidates"/> from the main scan cylinder. Used by
    /// <see cref="Softwyx.LootInVicinity.Ui.VicinityPanelPresenter.AttachNearbyPanelRoutine"/>.
    /// </summary>
    /// <param name="candidates">Loot piles to show in the vicinity panel; appended, not cleared.</param>
    /// <param name="seenItemIds">Item ids already listed this attach; skips duplicates across both passes.</param>
    /// <returns>Yields one frame, then runs the scan overlap pass.</returns>
    public static IEnumerator CollectCandidatesRoutine(List<LootItem> candidates, HashSet<string> seenItemIds){
        yield return null;

        var player = VicinityLocalPlayer.Find();

        if(!player) yield break;

        var mainRadius = Mathf.Max(Settings.ScanRadius.Value, MinMainScanRadius);

        PlayerCenterOfMass.GetMainScanCylinder(player, out var mainCenter, out var mainUp, out var mainHeight);

        CollectCylinder(
                        player,
                        mainCenter,
                        mainUp,
                        mainRadius,
                        mainHeight,
                        mainCenter,
                        mainUp,
                        mainHeight,
                        Settings.MainScanRequireLineOfSight.Value,
                        seenItemIds,
                        PassResults
                       );

        foreach(var loot in PassResults)
            if(loot)
                candidates.Add(loot);
    }

    private static void CollectCylinder(
        Player player, Vector3 center, Vector3 up, float radius, float height, Vector3 innerCenter, Vector3 innerUp,
        float  innerHeight, bool mainScanRequireLineOfSight, HashSet<string> seenItemIds, List<LootItem> output
    ){
        output.Clear();

        if(!player || radius <= 0f || height <= 0f) return;

        var count = VicinityCylinderOverlap.OverlapCylinderNonAlloc(
                                                                    center,
                                                                    up,
                                                                    radius,
                                                                    height,
                                                                    ColliderBuffer,
                                                                    LootScanLayers.ScanLayerMask
                                                                   );

        var profileId  = player.ProfileId;
        var seenInPass = new HashSet<LootItem>();
        var seenLoot   = new HashSet<int>();

        for(var i = 0; i < count; i++){
            var collider = ColliderBuffer[i];

            if(!collider) continue;

            var loot = collider.GetComponentInParent<LootItem>();

            if(!loot || !seenInPass.Add(loot)) continue;

            if(!seenLoot.Add(loot.GetInstanceID())) continue;

            if(loot.Item == null || !seenItemIds.Add(loot.Item.Id)) continue;

            if(!TryAcceptLoot(
                              player,
                              loot,
                              collider,
                              innerCenter,
                              innerUp,
                              InnerScanRadius,
                              innerHeight,
                              mainScanRequireLineOfSight,
                              profileId
                             ))
                continue;

            output.Add(loot);
        }
    }

    /// <summary>
    /// Per-collider filter after cylinder overlap (profile, optional <see cref="LineOfSight.CanSeeLoot"/>).
    /// </summary>
    /// <param name="player"></param>
    /// <param name="loot"></param>
    /// <param name="hitCollider"></param>
    /// <param name="innerCenter"></param>
    /// <param name="innerUp"></param>
    /// <param name="innerRadius"></param>
    /// <param name="innerHeight"></param>
    /// <param name="mainScanRequireLineOfSight"></param>
    /// <param name="profileId"></param>
    /// <returns>Whether this loot pile should be listed from the current scan pass.</returns>
    private static bool TryAcceptLoot(
        Player player, LootItem loot, Collider hitCollider, Vector3 innerCenter, Vector3 innerUp, float innerRadius,
        float  innerHeight, bool mainScanRequireLineOfSight, string profileId
    ){
        if(loot is Corpse) return false;

        if(!loot.isActiveAndEnabled || loot.Item == null) return false;

        if(!Settings.ShowQuestItemsInVicinity.Value && loot.Item.QuestItem) return false;

        if(!loot.IsValidForProfile(profileId)) return false;

        if(loot.Item is Weapon{
                            IsOneOff: true
                        } weapon
        && weapon.Repairable.Durability == 0f)
            return false;

        var samplePoint = hitCollider ? hitCollider.ClosestPoint(innerCenter) : loot.TrackableTransform.position;

        if(VicinityCylinderOverlap.IsInsideCylinder(samplePoint, innerCenter, innerUp, innerRadius, innerHeight))
            return true;

        if(!mainScanRequireLineOfSight || loot.Item.QuestItem) return true;

        var losCollider = hitCollider ?? loot.GetComponent<Collider>() ?? loot.GetComponentInChildren<Collider>();
        var lootPoint   = loot.TrackableTransform.position;

        return losCollider && LineOfSight.CanSeeLoot(player, losCollider, lootPoint);
    }
}
