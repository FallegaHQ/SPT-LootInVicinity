using System;
using EFT;

namespace Softwyx.LootInVicinity.Interop;

internal static class GameLocaleAccess{
    private const string DefaultLocaleId = "en";

    public static string TryLocalize(string key){
        if(string.IsNullOrEmpty(key)) return null;

        var manager = LocalizationManager.Instance;
        if(manager == null) return null;

        var localeId = manager.Culture ?? DefaultLocaleId;
        var tables = manager._locales;

        if(tables == null || !tables.TryGetValue(localeId, out var table) || table == null) return null;
        if(!table.TryGetValue(key, out var localized) || string.IsNullOrEmpty(localized)) return null;

        return string.Equals(localized, key, StringComparison.OrdinalIgnoreCase) ? null : localized;
    }
}
