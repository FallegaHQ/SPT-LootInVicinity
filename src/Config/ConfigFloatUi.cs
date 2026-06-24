using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx.Configuration;
using UnityEngine;

namespace Softwyx.LootInVicinity.Config;

/// <summary>
///     ConfigurationManager UI for <see cref="float" /> -- slider plus editable value field; commits with
///     step/min/max/decimals.
/// </summary>
internal static class ConfigFloatUi{
    private const           int                              DefaultDecimals = 2;
    private const           float                            LabelWidth      = 56f;
    private static readonly Dictionary<int, FloatFieldState> EditStates      = new();

    public static ConfigurationManagerAttributes Attributes(
        bool isAdvanced, int decimals = DefaultDecimals, float step = 0f
    ){
        return new ConfigurationManagerAttributes{
                                                     IsAdvanced   = isAdvanced,
                                                     CustomDrawer = SliderDrawer(decimals, step),
                                                     ObjToStr     = o => Format((float) o, decimals),
                                                     StrToObj     = s => ParseForStrToObj(s, decimals)
                                                 };
    }

    public static void SnapEntries(
        IEnumerable<ConfigEntryBase> entries, int decimals = DefaultDecimals, float step = 0f
    ){
        foreach(var entry in entries){
            if(entry is not ConfigEntry<float> f) continue;

            GetRange(f, out var min, out var max);

            f.Value = Quantize(f.Value, min, max, decimals, step);
        }
    }

    private static void GetRange(ConfigEntry<float> cfg, out float min, out float max){
        var range = cfg.Description.AcceptableValues as AcceptableValueRange<float>;

        min = range?.MinValue ?? 0f;
        max = range?.MaxValue ?? 1f;
    }

    private static int ResolveDecimals(int decimals, float step){
        return step > 0f ? Math.Max(decimals, DecimalsFromStep(step)) : decimals;
    }

    private static float Quantize(float value, float min, float max, int decimals, float step){
        if(step > 0f) value = SnapToStep(value, min, max, step);

        return Round(value, decimals);
    }

    private static float SnapToStep(float value, float min, float max, float step){
        var steps   = Mathf.Round((value - min) / step);
        var snapped = min + steps * step;

        return Mathf.Clamp(snapped, min, max);
    }

    private static int DecimalsFromStep(float step){
        if(step <= 0f) return DefaultDecimals;

        var text = step.ToString(CultureInfo.InvariantCulture);
        var dot  = text.IndexOf('.');

        return dot < 0 ? 0 : text.Length - dot - 1;
    }

    private static float Round(float value, int decimals){
        if(decimals <= 0) return Mathf.Round(value);

        var mul = Mathf.Pow(10f, decimals);

        return Mathf.Round(value * mul) / mul;
    }

    private static string Format(float value, int decimals){
        return Round(value, decimals).
            ToString("F" + decimals, CultureInfo.InvariantCulture);
    }

    private static float ParseForStrToObj(string text, int decimals){
        if(!float.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            throw new FormatException(text);

        return Round(value, decimals);
    }

    private static bool TryCommitText(
        ConfigEntry<float> cfg, ref string text, float min, float max, int decimals, float step
    ){
        if(string.IsNullOrWhiteSpace(text)){
            text = Format(Quantize(cfg.Value, min, max, decimals, step), decimals);

            return false;
        }

        if(!float.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var raw)){
            text = Format(Quantize(cfg.Value, min, max, decimals, step), decimals);

            return false;
        }

        var quantized = Quantize(raw, min, max, decimals, step);

        cfg.Value = quantized;
        text      = Format(quantized, decimals);

        return true;
    }

    private static FloatFieldState GetEditState(int id, float displayValue, int decimals){
        if(!EditStates.TryGetValue(id, out var state)){
            state = new FloatFieldState{
                                           Text = Format(displayValue, decimals)
                                       };

            EditStates[id] = state;
        }

        return state;
    }

    private static Action<ConfigEntryBase> SliderDrawer(int decimals, float step){
        return entry => {
                   var cfg = (ConfigEntry<float>) entry;

                   GetRange(cfg, out var min, out var max);

                   decimals = ResolveDecimals(decimals, step);

                   var id          = cfg.Definition.Key.GetHashCode();
                   var controlName = $"LIV_Float_{id}";
                   var focused     = GUI.GetNameOfFocusedControl() == controlName;
                   var state       = GetEditState(id, Quantize(cfg.Value, min, max, decimals, step), decimals);

                   if(state.WasFocused && !focused) TryCommitText(cfg, ref state.Text, min, max, decimals, step);

                   if(Event.current.type == EventType.KeyDown
                   && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                   && focused){
                       TryCommitText(cfg, ref state.Text, min, max, decimals, step);
                       GUI.FocusControl(null);
                       focused = false;
                   }

                   if(!focused) state.Text = Format(Quantize(cfg.Value, min, max, decimals, step), decimals);

                   var value = Quantize(cfg.Value, min, max, decimals, step);

                   if(!Mathf.Approximately(value, cfg.Value)) cfg.Value = value;

                   GUILayout.BeginHorizontal();

                   var next = GUILayout.HorizontalSlider(value, min, max);
                   next = Quantize(next, min, max, decimals, step);

                   if(!Mathf.Approximately(next, value)){
                       cfg.Value = next;

                       if(!focused) state.Text = Format(next, decimals);
                   }

                   GUI.SetNextControlName(controlName);
                   state.Text = GUILayout.TextField(state.Text, GUILayout.Width(LabelWidth));

                   state.WasFocused = GUI.GetNameOfFocusedControl() == controlName;

                   GUILayout.EndHorizontal();
               };
    }

    private sealed class FloatFieldState{
        public string Text;
        public bool   WasFocused;
    }
}
