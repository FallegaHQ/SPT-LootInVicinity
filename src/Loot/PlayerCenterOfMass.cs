using EFT;
using UnityEngine;

namespace Softwyx.LootInVicinity.Loot;

internal static class PlayerCenterOfMass{
    /// <summary>
    /// Origin for the main vicinity scan sphere in <see cref="Loot.VicinityLootScanner"/>.
    /// Uses <see cref="Player.InteractionRay"/> when available, else <see cref="GetBodyMidpointOrigin"/>.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>
    /// World position for overlap queries. <see cref="Vector3.zero"/> when <paramref name="player"/> is null.
    /// </returns>
    public static Vector3 GetMainScanSphereOrigin(Player player){
        if(!player) return Vector3.zero;

        return TryGetInteractionRayOrigin(player, out var rayOrigin) ? rayOrigin : GetBodyMidpointOrigin(player);
    }

    /// <summary>
    /// Fallback scan and LOS origin: midpoint between <see cref="GetFeetPosition"/> and <see cref="GetHeadPosition"/>.
    /// </summary>
    /// <param name="player"></param>
    /// <returns>
    /// Body midpoint. <see cref="Vector3.zero"/> when <paramref name="player"/> is null.
    /// </returns>
    private static Vector3 GetBodyMidpointOrigin(Player player){
        if(!player) return Vector3.zero;

        var feet = GetFeetPosition(player);
        var head = GetHeadPosition(player);

        return (feet + head) * 0.5f;
    }

    public static Vector3 GetKneeScanOrigin(Player player){
        if(!player) return Vector3.zero;

        var bones = player.PlayerBones;

        if(bones == null) return GetFeetPosition(player) + Vector3.up * 0.45f;

        if(bones.LeftThigh1 != null && bones.RightThigh1 != null)
            return (bones.LeftThigh1.position + bones.RightThigh1.position) * 0.5f;

        if(bones.LeftThigh1 != null) return bones.LeftThigh1.position;

        if(bones.RightThigh1 != null) return bones.RightThigh1.position;

        // This should never be the case unless some other mod messed up the bones.
        // No thigh bones; lower fallback than missing PlayerBones.
        return GetFeetPosition(player) + Vector3.up * 0.35f;
    }

    public static Vector3 GetLineOfSightOrigin(Player player){
        if(!player) return Vector3.zero;

        return TryGetInteractionRayOrigin(player, out var rayOrigin) ? rayOrigin : GetBodyMidpointOrigin(player);
    }

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
