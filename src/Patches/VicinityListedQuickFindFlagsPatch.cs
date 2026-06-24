using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Prefix on handler <see cref="InteractionsHandlerClass.QuickFindAppropriatePlace" /> --
///     sets quick-find flags.
/// </summary>
internal sealed class VicinityListedQuickFindFlagsPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(InteractionsHandlerClass),
                                  nameof(InteractionsHandlerClass.QuickFindAppropriatePlace),
                                  [
                                      typeof(Item), typeof(TraderControllerClass),
                                      typeof(IEnumerable<CompoundItem>),
                                      typeof(InteractionsHandlerClass.EMoveItemOrder), typeof(bool)
                                  ]
                                 );
    }

    [PatchPrefix]
    public static void PatchPrefix(
        Item item, TraderControllerClass controller, ref InteractionsHandlerClass.EMoveItemOrder order
    ){
        VicinityTakeFinalize.ApplyListedQuickFindFlags(item, controller, ref order);
    }
}
