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
    private const int   MaxColliderBuffer = 512;

    private static readonly Collider[]     ColliderBuffer = new Collider[MaxColliderBuffer];
    private static readonly List<LootItem> PassResults    = new(MaxColliderBuffer);

    /// <summary>
    /// Fills <paramref name="candidates"/> from main and feet-pocket cylinder scans. Used by
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

        PlayerCenterOfMass.GetMainScanCylinder(player, out var mainCenter, out var mainUp, out var mainHeight);

        CollectCylinder(
                        player,
                        mainCenter,
                        mainUp,
                        mainRadius,
                        mainHeight,
                        Settings.MainScanRequireLineOfSight.Value,
                        seenItemIds,
                        seenLoot,
                        PassResults
                       );

        foreach(var loot in PassResults)
            if(loot)
                candidates.Add(loot);

        PassResults.Clear();

        PlayerCenterOfMass.GetFeetPocketCylinder(
                                                 player,
                                                 out var feetCenter,
                                                 out var feetUp,
                                                 out var feetRadius,
                                                 out var feetHeight
                                                );

        CollectCylinder(player, feetCenter, feetUp, feetRadius, feetHeight, false, seenItemIds, seenLoot, PassResults);

        foreach(var loot in PassResults)
            if(loot)
                candidates.Add(loot);
    }

    private static void CollectCylinder(
        Player          player,      Vector3 center, Vector3 up, float radius, float height, bool requireLineOfSight,
        HashSet<string> seenItemIds, HashSet<int> seenLootInstances, List<LootItem> output
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

        for(var i = 0; i < count; i++){
            var collider = ColliderBuffer[i];

            if(!collider) continue;

            var loot = collider.GetComponentInParent<LootItem>();

            if(!loot || !seenInPass.Add(loot)) continue;

            if(!seenLootInstances.Add(loot.GetInstanceID())) continue;

            if(loot.Item == null || !seenItemIds.Add(loot.Item.Id)) continue;

            if(!TryAcceptLoot(player, loot, collider, requireLineOfSight, profileId)) continue;

            output.Add(loot);
        }
    }

    /// <summary>
    /// Per-collider filter after cylinder overlap (profile, optional <see cref="LineOfSight.CanSeeLoot"/>).
    /// </summary>
    /// <param name="player"></param>
    /// <param name="loot"></param>
    /// <param name="hitCollider">Collider that intersected the scan cylinder.</param>
    /// <param name="requireLineOfSight">When true, requires <see cref="LineOfSight.CanSeeLoot"/>.</param>
    /// <param name="profileId"></param>
    /// <returns>Whether this loot pile should be listed from the current scan pass.</returns>
    private static bool TryAcceptLoot(
        Player player, LootItem loot, Collider hitCollider, bool requireLineOfSight, string profileId
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

        if(!requireLineOfSight || loot.Item.QuestItem) return true;

        var losCollider = hitCollider ?? loot.GetComponent<Collider>() ?? loot.GetComponentInChildren<Collider>();
        var lootPoint   = loot.TrackableTransform.position;

        return losCollider && LineOfSight.CanSeeLoot(player, losCollider, lootPoint);
    }
}
