using System.Collections;
using System.Collections.Generic;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using Softwyx.LootInVicinity.LivPlayer;
using UnityEngine;

namespace Softwyx.LootInVicinity.Loot;

internal static class VicinityLootScanner{
    private const float MinMainScanRadius = 3f;
    private const float KneeScanRadius    = 1.5f;
    private const int   MaxColliderBuffer = 512;

    private static readonly Collider[]     ColliderBuffer = new Collider[MaxColliderBuffer];
    private static readonly List<LootItem> SphereResults  = new(MaxColliderBuffer);

    /// <summary>
    /// Fills <paramref name="candidates"/> from a main scan sphere and a smaller knee-height pass. Used by
    /// <see cref="Softwyx.LootInVicinity.Ui.VicinityPanelPresenter.AttachNearbyPanelRoutine"/>.
    /// </summary>
    /// <param name="candidates">Loot piles to show in the vicinity panel; appended, not cleared.</param>
    /// <param name="seenItemIds">Item ids already listed this attach; skips duplicates across both passes.</param>
    /// <returns>Yields one frame, then runs both overlap passes.</returns>
    public static IEnumerator CollectCandidatesRoutine(List<LootItem> candidates, HashSet<string> seenItemIds){
        yield return null;

        var player = VicinityLocalPlayer.Find();

        if(!player) yield break;

        var seenLoot = new HashSet<int>();

        var mainRadius = Mathf.Max(Settings.ScanRadius.Value, MinMainScanRadius);
        var mainOrigin = PlayerCenterOfMass.GetMainScanSphereOrigin(player);
        var kneeOrigin = PlayerCenterOfMass.GetKneeScanOrigin(player);

        CollectSphere(
                      player,
                      mainOrigin,
                      mainRadius,
                      Settings.MainScanRequireLineOfSight.Value,
                      seenItemIds,
                      seenLoot,
                      SphereResults
                     );

        foreach(var loot in SphereResults)
            if(loot)
                candidates.Add(loot);

        SphereResults.Clear();

        CollectSphere(player, kneeOrigin, KneeScanRadius, false, seenItemIds, seenLoot, SphereResults);

        foreach(var loot in SphereResults)
            if(loot)
                candidates.Add(loot);

        if(candidates.Count > 1) SortByDistanceFrom(mainOrigin, candidates);
    }

    private static void SortByDistanceFrom(Vector3 origin, List<LootItem> candidates){
        candidates.Sort(
                        (a, b) => {
                            var da = DistanceSqrFromOrigin(origin, a);
                            var db = DistanceSqrFromOrigin(origin, b);

                            return da.CompareTo(db);
                        }
                       );
    }

    private static float DistanceSqrFromOrigin(Vector3 origin, LootItem loot){
        if(!loot) return float.MaxValue;

        var collider = loot.GetComponent<Collider>() ?? loot.GetComponentInChildren<Collider>();

        var samplePoint = collider ? collider.ClosestPoint(origin) : loot.TrackableTransform.position;

        return (samplePoint - origin).sqrMagnitude;
    }

    private static void CollectSphere(
        Player       player, Vector3 origin, float radius, bool requireLineOfSight, HashSet<string> seenItemIds,
        HashSet<int> seenLootInstances, List<LootItem> output
    ){
        output.Clear();

        if(!player || radius <= 0f) return;

        var count = Physics.OverlapSphereNonAlloc(
                                                  origin,
                                                  radius,
                                                  ColliderBuffer,
                                                  LootScanLayers.ScanLayerMask,
                                                  QueryTriggerInteraction.Collide
                                                 );

        var profileId  = player.ProfileId;
        var radiusSqr  = radius * radius;
        var seenInPass = new HashSet<LootItem>();

        for(var i = 0; i < count; i++){
            var collider = ColliderBuffer[i];

            if(!collider) continue;

            var loot = collider.GetComponentInParent<LootItem>();

            if(!loot || !seenInPass.Add(loot)) continue;

            if(!seenLootInstances.Add(loot.GetInstanceID())) continue;

            if(loot.Item == null || !seenItemIds.Add(loot.Item.Id)) continue;

            if(!TryAcceptLoot(player, loot, collider, origin, radiusSqr, profileId, requireLineOfSight)) continue;

            output.Add(loot);
        }
    }

    /// <summary>
    /// Per-collider filter after <see cref="CollectSphere"/> overlap hit (distance, profile, optional
    /// <see cref="LineOfSight.CanSeeLoot"/>).
    /// </summary>
    /// <param name="player"></param>
    /// <param name="loot"></param>
    /// <param name="hitCollider">Collider from the overlap pass; used for distance and line of sight.</param>
    /// <param name="origin">Centre of the scan sphere for this pass.</param>
    /// <param name="radiusSqr">Squared radius for <paramref name="origin"/>.</param>
    /// <param name="profileId"></param>
    /// <param name="requireLineOfSight">When true, requires <see cref="LineOfSight.CanSeeLoot"/>.</param>
    /// <returns>Whether this loot pile should be listed from the current scan pass.</returns>
    private static bool TryAcceptLoot(
        Player player, LootItem loot, Collider hitCollider, Vector3 origin, float radiusSqr, string profileId,
        bool   requireLineOfSight
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

        var losCollider = hitCollider ?? loot.GetComponent<Collider>() ?? loot.GetComponentInChildren<Collider>();
        var samplePoint = losCollider ? losCollider.ClosestPoint(origin) : loot.TrackableTransform.position;
        var delta       = samplePoint - origin;

        if(delta.sqrMagnitude > radiusSqr) return false;

        if(!requireLineOfSight || loot.Item.QuestItem) return true;

        var lootPoint = loot.TrackableTransform.position;

        return losCollider && LineOfSight.CanSeeLoot(player, losCollider, lootPoint);
    }
}
