using System.Diagnostics.CodeAnalysis;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Prefix on <see cref="InteractionsHandlerClass.Discard"/> --
/// blocks discard for items listed in the vicinity panel grid.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityDiscardPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(InteractionsHandlerClass),
                                  nameof(InteractionsHandlerClass.Discard),
                                  [typeof(Item), typeof(TraderControllerClass), typeof(bool)]
                                 );
    }

    [PatchPrefix]
    public static bool PatchPrefix(Item item, ref DiscardResult __result){
        if(!VicinityDiscardGuard.ShouldBlockDiscard(item)) return true;

        __result = new InventoryStringError("Cannot discard from vicinity panel");

        return false;
    }
}
