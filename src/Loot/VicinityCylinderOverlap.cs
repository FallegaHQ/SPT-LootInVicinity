using UnityEngine;

namespace Softwyx.LootInVicinity.Loot;

/// <summary>Capsule query plus flat-ended cylinder test (see <see cref="OverlapCylinderNonAlloc"/>).</summary>
internal static class VicinityCylinderOverlap{
    /// <summary>
    /// Fills <paramref name="results"/> with colliders inside a flat-ended cylinder. Uses
    /// <see cref="Physics.OverlapCapsuleNonAlloc(Vector3,Vector3,float,Collider[],int,QueryTriggerInteraction)"/>
    /// then <see cref="IsInsideCylinder"/> on each hit's closest point.
    /// </summary>
    /// <returns>Number of colliders written to <paramref name="results"/>.</returns>
    public static int OverlapCylinderNonAlloc(
        Vector3 center, Vector3 up, float radius, float height, Collider[] results, int layerMask
    ){
        if(results == null || results.Length == 0 || radius <= 0f || height <= 0f) return 0;

        up = up.sqrMagnitude > 0.0001f ? up.normalized : Vector3.up;

        var innerHalf = Mathf.Max(height * 0.5f - radius, 0f);
        var p0        = center + up * innerHalf;
        var p1        = center - up * innerHalf;

        var count = Physics.OverlapCapsuleNonAlloc(p0, p1, radius, results, layerMask, QueryTriggerInteraction.Collide);

        var validCount = 0;

        for(var i = 0; i < count; i++){
            var collider = results[i];

            if(!collider) continue;

            var closest = collider.ClosestPoint(center);

            if(!IsInsideCylinder(closest, center, up, radius, height)) continue;

            results[validCount++] = collider;
        }

        return validCount;
    }

    /// <summary>
    /// Whether <paramref name="point"/> lies inside a cylinder aligned on <paramref name="up"/>.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="center"></param>
    /// <param name="up"></param>
    /// <param name="radius"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    internal static bool IsInsideCylinder(Vector3 point, Vector3 center, Vector3 up, float radius, float height){
        var delta     = point - center;
        var axialDist = Vector3.Dot(delta, up);

        if(Mathf.Abs(axialDist) > height * 0.5f) return false;

        var radial = delta - up * axialDist;

        return radial.sqrMagnitude <= radius * radius;
    }
}
