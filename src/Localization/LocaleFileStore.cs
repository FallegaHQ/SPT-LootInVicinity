using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Softwyx.LootInVicinity.Localization;

internal static class LocaleFileStore{
    public const string DefaultLocaleId = "en";

    private static readonly Regex LocaleIdPattern = new(
                                                        "^[a-z]{2}([_-][a-z]{2})?$",
                                                        RegexOptions.Compiled | RegexOptions.CultureInvariant
                                                       );

    private static readonly Regex KeyPattern = new(
                                                   $"^{Regex.Escape(LocaleKeys.KeyPrefix)}[a-z0-9_]+$",
                                                   RegexOptions.Compiled | RegexOptions.CultureInvariant
                                                  );

    private static readonly JsonSerializerSettings SerializerSettings = new(){
                                                                                 NullValueHandling =
                                                                                     NullValueHandling.Ignore
                                                                             };

    public static string LocalesDirectory => PluginPaths.LocalesDirectory;

    public static IReadOnlyList<string> DiscoverLocaleIds(){
        if(!Directory.Exists(LocalesDirectory)) return [];

        var ids = new List<string>();

        foreach(var path in Directory.GetFiles(LocalesDirectory, "*.json")){
            var id = Path.GetFileNameWithoutExtension(path);

            if(!IsValidLocaleId(id)) continue;

            ids.Add(id);
        }

        ids.Sort(StringComparer.OrdinalIgnoreCase);

        return ids;
    }

    public static bool TryLoadFile(string localeId, out Dictionary<string, string> entries, out string error){
        entries = null;
        error   = null;

        if(!IsValidLocaleId(localeId)){
            error = $"Invalid locale id '{localeId}'.";

            return false;
        }

        var path = Path.Combine(LocalesDirectory, $"{localeId.Trim().ToLowerInvariant()}.json");

        if(File.Exists(path)) return TryReadValidatedFile(path, out entries, out error);

        error = $"Locale file not found: {path}";

        return false;
    }

    private static bool IsValidLocaleId(string localeId){
        return !string.IsNullOrWhiteSpace(localeId) && LocaleIdPattern.IsMatch(localeId.Trim());
    }

    public static string NormalizeLocaleId(string localeId){
        if(string.IsNullOrWhiteSpace(localeId)) return DefaultLocaleId;

        var trimmed = localeId.Trim().
                               ToLowerInvariant();

        return IsValidLocaleId(trimmed) ? trimmed : DefaultLocaleId;
    }

    private static bool TryReadValidatedFile(string path, out Dictionary<string, string> entries, out string error){
        entries = null;
        error   = null;

        try{
            var raw = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                                                                                File.ReadAllText(path),
                                                                                SerializerSettings
                                                                               );

            if(raw == null){
                error = $"Locale file is empty or invalid: {path}";

                return false;
            }

            entries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach(var pair in raw){
                if(!TryValidateEntry(pair.Key, pair.Value, out var entryError)){
                    error = $"{Path.GetFileName(path)}: {entryError}";

                    return false;
                }

                entries[pair.Key] = pair.Value;
            }

            return entries.Count > 0;
        }
        catch(Exception ex){
            error = $"Failed to read {path}: {ex.Message}";

            return false;
        }
    }

    private static bool TryValidateEntry(string key, string value, out string error){
        error = null;

        if(string.IsNullOrWhiteSpace(key)){
            error = "entry key is empty";

            return false;
        }

        if(!KeyPattern.IsMatch(key)){
            error = $"key '{key}' must match {LocaleKeys.KeyPrefix}*";

            return false;
        }

        if(value != null) return true;

        error = $"key '{key}' has null value";

        return false;
    }
}
