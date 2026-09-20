using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using Softwyx.LootInVicinity.Take;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

/// <summary>
///     Prefix on handler <see cref="EFT.InventoryLogic.ItemManipulator.QuickFindAppropriatePlace" /> --
///     sets quick-find flags.
/// </summary>
internal sealed class VicinityListedQuickFindFlagsPatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(ItemManipulator),
                                  nameof(ItemManipulator.QuickFindAppropriatePlace),
                                  [
                                      typeof(Item), typeof(ItemController),
                                      typeof(IEnumerable<CompoundItem>),
                                      typeof(ItemManipulator.EMoveItemOrder), typeof(bool)
                                  ]
                                 );
    }

    [PatchPrefix]
    public static void PatchPrefix(
        Item item, ItemController controller, ref ItemManipulator.EMoveItemOrder order
    ){
        VicinityTakeFinalize.ApplyListedQuickFindFlags(item, controller, ref order);
    }
}
