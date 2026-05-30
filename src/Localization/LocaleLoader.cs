using System;
using System.Collections.Generic;
using System.Linq;

namespace Softwyx.LootInVicinity.Localization;

internal static class LocaleLoader{
    private static readonly List<string> AppliedToGame = [];
    private static Dictionary<string, Dictionary<string, string>> _catalogue = new(StringComparer.OrdinalIgnoreCase);

    private static bool IsInitialized => _catalogue.Count > 0;

    public static bool PreloadDefaultLocale(out string error){
        error = null;

        if(!TryBuildCatalog(out error)) return false;

        ApplyValidatedCatalogToGame();

        return true;
    }

    public static void LoadLocale(string localeId){
        if(!IsInitialized || string.IsNullOrWhiteSpace(localeId)) return;

        localeId = LocaleFileStore.NormalizeLocaleId(localeId);

        if(AppliedToGame.Contains(localeId)) return;

        if(!_catalogue.TryGetValue(localeId, out var localeDict)) return;

        var manager = LocaleManagerClass.LocaleManagerClass;

        if(!manager.ContainsCulture(localeId)) return;

        manager.UpdateLocales(localeId, CopyDictionary(localeDict));
        AppliedToGame.Add(localeId);

        LootInVicinityPlugin.Log?.LogInfo(
                                          PluginInfo.Format(
                                                            $"Applied locale '{localeId}' ({localeDict.Count} entries)."
                                                           )
                                         );
    }

    public static string Get(string key){
        if(string.IsNullOrEmpty(key)) return string.Empty;

        var fromPack = TryGet(key);

        if(!string.IsNullOrEmpty(fromPack)) return fromPack;

        var localized = GameLocaleAccess.TryLocalize(key);

        return !string.IsNullOrEmpty(localized) ? localized : key;
    }

    public static string Get(string key, params object[] args){
        var text = Get(key);

        return args == null || args.Length == 0 ? text : string.Format(text, args);
    }

    public static string TryGet(string key){
        if(string.IsNullOrEmpty(key)) return null;

        return !_catalogue.TryGetValue(LocaleFileStore.DefaultLocaleId, out var dict)
                   ? null
                   : dict.GetValueOrDefault(key);
    }

    private static bool TryBuildCatalog(out string error){
        error      = null;
        _catalogue = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        var discoveredIds = LocaleFileStore.DiscoverLocaleIds();

        if(discoveredIds.Count == 0){
            error = $"No locale files in '{LocaleFileStore.LocalesDirectory}'.";

            return false;
        }

        if(!discoveredIds.Any(
                              id => string.Equals(
                                                  id,
                                                  LocaleFileStore.DefaultLocaleId,
                                                  StringComparison.OrdinalIgnoreCase
                                                 )
                             )){
            error = $"Required '{LocaleFileStore.DefaultLocaleId}.json' is missing from locale packs.";

            return false;
        }

        Dictionary<string, string> english = null;

        foreach(var localeId in discoveredIds){
            if(!LocaleFileStore.TryLoadFile(localeId, out var entries, out var loadError)){
                if(string.Equals(localeId, LocaleFileStore.DefaultLocaleId, StringComparison.OrdinalIgnoreCase)){
                    error = loadError;

                    return false;
                }

                LootInVicinityPlugin.Log?.LogWarning(
                                                     PluginInfo.Format(
                                                                       $"Skipping invalid locale '{localeId}': {loadError}"
                                                                      )
                                                    );

                continue;
            }

            if(entries.Count == 0){
                if(string.Equals(localeId, LocaleFileStore.DefaultLocaleId, StringComparison.OrdinalIgnoreCase)){
                    error = $"Locale '{localeId}' is empty.";

                    return false;
                }

                LootInVicinityPlugin.Log?.LogWarning(PluginInfo.Format($"Skipping empty locale '{localeId}'."));

                continue;
            }

            _catalogue[localeId] = entries;

            if(string.Equals(localeId, LocaleFileStore.DefaultLocaleId, StringComparison.OrdinalIgnoreCase))
                english = entries;
        }

        if(english == null){
            error = $"Locale '{LocaleFileStore.DefaultLocaleId}' failed validation.";

            return false;
        }

        foreach(var pair in _catalogue){
            if(string.Equals(pair.Key, LocaleFileStore.DefaultLocaleId, StringComparison.OrdinalIgnoreCase)) continue;

            MergeMissingKeys(pair.Key, pair.Value, english);
        }

        LootInVicinityPlugin.Log?.LogInfo(
                                          PluginInfo.Format(
                                                            $"Validated locale packs: {string.Join(", ", _catalogue.Keys.OrderBy(id => id, StringComparer.OrdinalIgnoreCase))}."
                                                           )
                                         );

        return true;
    }

    private static void ApplyValidatedCatalogToGame(){
        AppliedToGame.Clear();

        var manager = LocaleManagerClass.LocaleManagerClass;

        foreach(var pair in _catalogue.OrderBy(static p => p.Key, StringComparer.OrdinalIgnoreCase)){
            manager.UpdateLocales(pair.Key, CopyDictionary(pair.Value));
            AppliedToGame.Add(pair.Key);

            LootInVicinityPlugin.Log?.LogInfo(
                                              PluginInfo.Format(
                                                                $"Merged {pair.Value.Count} locale entries for '{pair.Key}'."
                                                               )
                                             );
        }
    }

    private static void MergeMissingKeys(
        string localeId, Dictionary<string, string> localeDict, Dictionary<string, string> english
    ){
        foreach(var englishEntry in english){
            if(localeDict.ContainsKey(englishEntry.Key)) continue;

            LootInVicinityPlugin.Log?.LogWarning(
                                                 PluginInfo.Format(
                                                                   $"Locale '{localeId}' is missing entry '{englishEntry.Key}'."
                                                                  )
                                                );
            localeDict[englishEntry.Key] = englishEntry.Value;
        }
    }

    private static Dictionary<string, string> CopyDictionary(Dictionary<string, string> source){
        return new Dictionary<string, string>(source, StringComparer.OrdinalIgnoreCase);
    }
}
