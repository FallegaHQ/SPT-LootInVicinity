using System;
using System.Collections.Generic;
using EFT;
using EFT.Interactive;
using UnityEngine;

namespace Softwyx.LootInVicinity.Loot;

internal static class LineOfSight{
    private static readonly RaycastHit[] InteractiveHits = new RaycastHit[32];

    /// <summary>
    /// Line of sight for main scan. Uses <see cref="GameWorld.LootMaskObstruction"/> then
    /// interactive ray hits on <see cref="GameWorld.InteractiveLootMaskWPlayer"/>.
    /// Nearest blocking hit must be <paramref name="lootCollider"/> or its parent or child.
    /// Other <see cref="LootItem"/> piles and <see cref="Corpse"/> do not block.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="lootCollider"></param>
    /// <param name="lootPoint"></param>
    /// <returns>Whether the loot is visible from the player along the vanilla LOS masks.</returns>
    public static bool CanSeeLoot(Player player, Collider lootCollider, Vector3 lootPoint){
        if(!player || !lootCollider) return false;

        var eye = PlayerCenterOfMass.GetLineOfSightOrigin(player);

        // Walls and doors; checked before which interactive collider was hit.
        if(GameWorld.LootMaskObstruction != 0 && Physics.Linecast(eye, lootPoint, GameWorld.LootMaskObstruction))
            return false;

        var delta    = lootPoint - eye;
        var distance = delta.magnitude;

        if(distance <= 0.0001f) return true;

        var direction = delta / distance;
        var hitCount = Physics.RaycastNonAlloc(
                                               eye,
                                               direction,
                                               InteractiveHits,
                                               distance,
                                               GameWorld.InteractiveLootMaskWPlayer,
                                               QueryTriggerInteraction.Collide
                                              );

        if(hitCount <= 0) return true;

        Array.Sort(InteractiveHits, 0, hitCount, RaycastHitDistanceComparer.Instance);

        for(var i = 0; i < hitCount; i++){
            var hitCollider = InteractiveHits[i].collider;

            if(!hitCollider) continue;

            if(IsTargetLootCollider(hitCollider, lootCollider)) return true;

            if(IsOtherLootOrCorpse(hitCollider, lootCollider)) continue;

            return false;
        }

        return true;
    }

    private static bool IsTargetLootCollider(Collider hit, Collider lootCollider){
        return hit == lootCollider
            || hit.transform.IsChildOf(lootCollider.transform)
            || lootCollider.transform.IsChildOf(hit.transform);
    }

    private static bool IsOtherLootOrCorpse(Collider hit, Collider targetLootCollider){
        if(IsTargetLootCollider(hit, targetLootCollider)) return false;

        return hit.GetComponentInParent<LootItem>() != null;
    }

    private sealed class RaycastHitDistanceComparer : IComparer<RaycastHit>{
        public static readonly RaycastHitDistanceComparer Instance = new();

        public int Compare(RaycastHit a, RaycastHit b){
            return a.distance.CompareTo(b.distance);
        }
    }
}
