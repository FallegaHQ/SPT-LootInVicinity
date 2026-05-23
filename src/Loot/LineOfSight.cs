using EFT;
using UnityEngine;

namespace Softwyx.LootInVicinity.Loot;

internal static class LineOfSight{
    /// <summary>
    /// Line of sight for main scan. Uses <see cref="GameWorld.LootMaskObstruction"/> then
    /// <see cref="NearestHitRaycast.GetNearestHit(UnityEngine.Vector3,UnityEngine.Vector3,out UnityEngine.RaycastHit,float,int)"/>
    /// on <see cref="GameWorld.InteractiveLootMaskWPlayer"/>.
    /// Nearest hit must be <paramref name="lootCollider"/> or its parent or child.
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

        var distance = Vector3.Distance(eye, lootPoint);

        if(!NearestHitRaycast.GetNearestHit(
                                            eye,
                                            lootPoint,
                                            out var hit,
                                            distance,
                                            GameWorld.InteractiveLootMaskWPlayer
                                           ))
            return true; // nothing interactive between eye and loot

        if(!hit.collider) return true;

        // Hit collider may be on a parent or child of the LootItem collider we were given.
        return hit.collider == lootCollider
            || hit.collider.transform.IsChildOf(lootCollider.transform)
            || lootCollider.transform.IsChildOf(hit.collider.transform);
    }
}
