using UnityEngine;

namespace Softwyx.LootInVicinity.Loot;

internal static class LootScanLayers{
    private static bool _maskDirty = true;

    private static int LootLayerIndex{
        get{
            if(field >= 0) return field;

            field = LayerMask.NameToLayer("Loot");

            if(field < 0) field = 15;

            return field;
        }
    } = -1;

    internal static int ScanLayerMask{
        get{
            if(!_maskDirty) return field;

            field = LayerMask.GetMask("Interactive");

            if(field == 0){
                var interactive = LayerMask.NameToLayer("Interactive");

                if(interactive < 0) interactive = 22;

                field = 1 << interactive;
            }

            field |= 1 << LootLayerIndex;

            _maskDirty = false;

            return field;
        }
    }
}
