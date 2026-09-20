using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT;
using HarmonyLib;
using Softwyx.LootInVicinity.Localization;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class LocaleApplicationLanguagePatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(LocalizationManager),
                                  nameof(LocalizationManager.UpdateApplicationLanguage)
                                 );
    }

    [PatchPostfix]
    private static void Postfix(LocalizationManager __instance){
        if(__instance == null) return;

        var localeId = __instance.Culture;

        LocaleLoader.LoadLocale(localeId);
    }
}
