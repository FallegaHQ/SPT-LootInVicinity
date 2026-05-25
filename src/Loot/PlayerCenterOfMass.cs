using EFT;
using UnityEngine;

namespace Softwyx.LootInVicinity.Loot;

/// <summary>Player-space anchors for vicinity scan cylinders and line-of-sight origin.</summary>
internal static class PlayerCenterOfMass{
    private const float MainBelowFeetMeters       = 0.5f;
    private const float MainAboveHeadMeters       = 1f;
    private const float FeetPocketRadiusMeters    = 1f;
    private const float FeetPocketCenterAboveFeet = 0.2f;
    private const float FeetPocketHeightMeters    = 1f;

    /// <summary>
    /// Vertical axis for <see cref="VicinityCylinderOverlap"/> queries on
    /// <paramref name="player"/>.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>
    /// Normalized <see cref="Transform.up"/> on the player, or <see cref="Vector3.up"/> when
    /// <paramref name="player"/> or its transform is missing.
    /// </returns>
    private static Vector3 GetPlayerUp(Player player){
        return player?.Transform != null ? player.Transform.up.normalized : Vector3.up;
    }

    /// <summary>
    /// Main scan cylinder: from feet minus 0.5 m to head plus 1 m along
    /// <see cref="GetPlayerUp"/>.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="center">Midpoint between the flat bottom and top caps.</param>
    /// <param name="up">Cylinder axis from <see cref="GetPlayerUp"/>.</param>
    /// <param name="height">Extent along <paramref name="up"/> between the caps.</param>
    public static void GetMainScanCylinder(Player player, out Vector3 center, out Vector3 up, out float height){
        GetCylinderFromFeetAndHead(player, MainBelowFeetMeters, MainAboveHeadMeters, out center, out up, out height);
    }

    /// <summary>
    /// Feet-pocket scan cylinder: 1 m radius, 1 m tall, center 0.2 m above
    /// <see cref="GetFeetPosition"/>.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="center">Center of the feet-pocket volume.</param>
    /// <param name="up">Cylinder axis from <see cref="GetPlayerUp"/>.</param>
    /// <param name="radius">Fixed 1 m.</param>
    /// <param name="height">Fixed 1 m.</param>
    public static void GetFeetPocketCylinder(
        Player player, out Vector3 center, out Vector3 up, out float radius, out float height
    ){
        var feet = GetFeetPosition(player);

        up     = GetPlayerUp(player);
        center = feet + up * FeetPocketCenterAboveFeet;
        radius = FeetPocketRadiusMeters;
        height = FeetPocketHeightMeters;
    }

    /// <summary>
    /// Eye position for <see cref="LineOfSight.CanSeeLoot"/>. Uses
    /// <see cref="Player.InteractionRay"/> when alive and ready, else body midpoint.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>
    /// World origin for LOS rays, or <see cref="Vector3.zero"/> when <paramref name="player"/> is null.
    /// </returns>
    public static Vector3 GetLineOfSightOrigin(Player player){
        if(!player) return Vector3.zero;

        return TryGetInteractionRayOrigin(player, out var rayOrigin) ? rayOrigin : GetBodyMidpointOrigin(player);
    }

    /// <summary>
    /// Builds a vertical cylinder from feet and head anchors along <see cref="GetPlayerUp"/>.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="belowFeet">Meters below <see cref="GetFeetPosition"/> along <paramref name="up"/>.</param>
    /// <param name="aboveHead">Meters above <see cref="GetHeadPosition"/> along <paramref name="up"/>.</param>
    /// <param name="center">Midpoint between bottom and top caps.</param>
    /// <param name="up">Cylinder axis from <see cref="GetPlayerUp"/>.</param>
    /// <param name="height">Extent along <paramref name="up"/> between the caps.</param>
    private static void GetCylinderFromFeetAndHead(
        Player player, float belowFeet, float aboveHead, out Vector3 center, out Vector3 up, out float height
    ){
        var feet = GetFeetPosition(player);
        var head = GetHeadPosition(player);

        up = GetPlayerUp(player);

        var bottom = feet - up * belowFeet;
        var top    = head + up * aboveHead;

        center = (bottom + top) * 0.5f;
        height = Vector3.Dot(top - bottom, up);
    }

    /// <summary>
    /// Fallback LOS and scan anchor: midpoint between feet and head.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>
    /// Average of <see cref="GetFeetPosition"/> and <see cref="GetHeadPosition"/>, or
    /// <see cref="Vector3.zero"/> when <paramref name="player"/> is null.
    /// </returns>
    private static Vector3 GetBodyMidpointOrigin(Player player){
        if(!player) return Vector3.zero;

        var feet = GetFeetPosition(player);
        var head = GetHeadPosition(player);

        return (feet + head) * 0.5f;
    }

    /// <summary>
    /// Reads <see cref="Player.InteractionRay"/> origin for LOS when the ray is available.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="origin">Ray origin when the method succeeds.</param>
    /// <returns>Whether <paramref name="origin"/> was set from the interaction ray.</returns>
    private static bool TryGetInteractionRayOrigin(Player player, out Vector3 origin){
        origin = default;

        if(!player.HealthController.IsAlive) return false;

        try{
            origin = player.InteractionRay.origin;

            return true;
        }
        catch{
            // InteractionRay not ready (e.g. early raid spawn, probably).
            return false;
        }
    }

    private static Vector3 GetFeetPosition(Player player){
        if(player.PlayerBones?.BodyTransform != null) return player.PlayerBones.BodyTransform.position;

        return player.Transform?.position ?? player.Position;
    }

    private static Vector3 GetHeadPosition(Player player){
        if(player.MainParts != null
        && player.MainParts.TryGetValue(BodyPartType.head, out var headPart)
        && headPart != null)
            return headPart.Position;

        if(player.PlayerBones?.Head != null) return player.PlayerBones.Head.position;

        // Roughly, return the lower neck position
        return GetFeetPosition(player) + Vector3.up * 1.65f;
    }
}
