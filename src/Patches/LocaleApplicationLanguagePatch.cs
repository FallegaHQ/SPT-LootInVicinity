using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Softwyx.LootInVicinity.Patches;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class LocaleApplicationLanguagePatch : ModulePatch{
    protected override MethodBase GetTargetMethod(){
        return AccessTools.Method(
                                  typeof(LocalizationManager),
                                  GameAssemblyNames.LocaleManagerMethods.UpdateApplicationLanguage
                                 );
    }

    [PatchPostfix]
    private static void Postfix(LocalizationManager __instance){
        if(__instance == null) return;

        var localeId = Traverse.Create(__instance).
                                Property(GameAssemblyNames.LocaleManagerProperties.SelectedLanguage).
                                GetValue<string>();

        LocaleLoader.LoadLocale(localeId);
    }
}
