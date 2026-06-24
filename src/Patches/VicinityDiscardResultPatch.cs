using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Postfix on <see cref="InteractionsHandlerClass.Discard" /> -- cleans up consumed vicinity loot.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class VicinityDiscardResultPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(InteractionsHandlerClass),
                                  nameof(InteractionsHandlerClass.Discard),
                                  [typeof(Item), typeof(TraderControllerClass), typeof(bool)]
                                 );
    }

    [PatchPostfix]
    public static void PatchPostfix(Item item, bool simulate, ref DiscardResult __result){
        VicinityListedWorldCleanup.TryCleanupAfterInventoryMutation(item, simulate, __result.Succeeded);
    }
}
