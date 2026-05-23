using UnityEngine;

namespace Softwyx.LootInVicinity.Loot;

internal static class LootScanLayers{
    private static int  _lootLayer = -1;
    private static int  _cachedMask;
    private static bool _maskDirty = true;

    private static int LootLayerIndex{
        get{
            if(_lootLayer >= 0) return _lootLayer;

            _lootLayer = LayerMask.NameToLayer("Loot");

            if(_lootLayer < 0) _lootLayer = 15;

            return _lootLayer;
        }
    }

    internal static int ScanLayerMask{
        get{
            if(!_maskDirty) return _cachedMask;

            _cachedMask = LayerMask.GetMask("Interactive");

            if(_cachedMask == 0){
                var interactive = LayerMask.NameToLayer("Interactive");

                if(interactive < 0) interactive = 22;

                _cachedMask = 1 << interactive;
            }

            _cachedMask |= 1 << LootLayerIndex;

            _maskDirty = false;

            return _cachedMask;
        }
    }
}
