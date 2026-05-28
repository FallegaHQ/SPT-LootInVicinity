using System;
using System.Collections.Generic;
using HarmonyLib;

namespace Softwyx.LootInVicinity.Interop;

internal static class GameLocaleAccess{
    public static string TryLocalize(string key){
        if(string.IsNullOrEmpty(key)) return null;

        try{
            var manager = LocaleManagerClass.LocaleManagerClass;

            if(manager == null) return null;

            var localeId = Traverse.Create(manager).
                                    Property(GameAssemblyNames.LocaleManagerProperties.SelectedLanguage).
                                    GetValue<string>();

            if(string.IsNullOrEmpty(localeId)) localeId = LocaleFileStore.DefaultLocaleId;

            var tables = Traverse.Create(manager).
                                  Field(GameAssemblyNames.LocaleManagerFields.LocaleTables).
                                  GetValue<Dictionary<string, Dictionary<string, string>>>();

            if(tables == null || !tables.TryGetValue(localeId, out var table) || table == null) return null;

            if(!table.TryGetValue(key, out var localized) || string.IsNullOrEmpty(localized)) return null;

            return string.Equals(localized, key, StringComparison.OrdinalIgnoreCase) ? null : localized;
        }
        catch(Exception ex){
            LootInVicinityPlugin.Log?.LogDebug(
                                               PluginInfo.Format($"Game locale lookup failed for '{key}': {ex.Message}")
                                              );

            return null;
        }
    }
}
