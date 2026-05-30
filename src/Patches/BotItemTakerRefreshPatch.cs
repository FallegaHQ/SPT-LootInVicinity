using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Finalizer on <see cref="BotItemTaker.RefreshClosestItems"/> --
/// suppresses stale-loot NRE after vicinity GO-only destroy.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class BotItemTakerRefreshPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(typeof(BotItemTaker), nameof(BotItemTaker.RefreshClosestItems));
    }

    [PatchFinalizer]
    public static Exception PatchFinalizer(Exception __exception){
        if(__exception is NullReferenceException && Settings.Enabled.Value) return null;

        return __exception;
    }
}
