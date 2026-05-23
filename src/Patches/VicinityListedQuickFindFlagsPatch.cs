using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>Prefix on handler <see cref="InteractionsHandlerClass.QuickFindAppropriatePlace"/> --
/// sets quick-find flags.</summary>
internal sealed class VicinityListedQuickFindFlagsPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(InteractionsHandlerClass),
                                  nameof(InteractionsHandlerClass.QuickFindAppropriatePlace),
                                  [
                                      typeof(Item), typeof(TraderControllerClass),
                                      typeof(System.Collections.Generic.IEnumerable<CompoundItem>),
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
