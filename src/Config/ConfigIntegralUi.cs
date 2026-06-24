using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx.Configuration;
using UnityEngine;

namespace Softwyx.LootInVicinity.Config;

internal static class ConfigIntegralUi{
    private const float LabelWidthInt  = 56f;
    private const float LabelWidthLong = 72f;

    private static readonly Dictionary<int, IntFieldState>  IntEditStates  = new();
    private static readonly Dictionary<int, LongFieldState> LongEditStates = new();

    public static ConfigurationManagerAttributes IntAttributes(bool isAdvanced, int step = 0){
        return new ConfigurationManagerAttributes{
                                                     IsAdvanced   = isAdvanced,
                                                     CustomDrawer = IntSliderDrawer(step),
                                                     ObjToStr     = o => Format((int) o),
                                                     StrToObj     = s => ParseInt(s)
                                                 };
    }

    public static ConfigurationManagerAttributes LongAttributes(bool isAdvanced, long step = 0){
        return new ConfigurationManagerAttributes{
                                                     IsAdvanced   = isAdvanced,
                                                     CustomDrawer = LongSliderDrawer(step),
                                                     ObjToStr     = o => Format((long) o),
                                                     StrToObj     = s => ParseLong(s)
                                                 };
    }

    private static void GetRange(ConfigEntry<int> cfg, out int min, out int max){
        var range = cfg.Description.AcceptableValues as AcceptableValueRange<int>;

        min = range?.MinValue ?? 0;
        max = range?.MaxValue ?? 100;
    }

    private static void GetRange(ConfigEntry<long> cfg, out long min, out long max){
        var range = cfg.Description.AcceptableValues as AcceptableValueRange<long>;

        min = range?.MinValue ?? 0L;
        max = range?.MaxValue ?? 100L;
    }

    private static int Quantize(int value, int min, int max, int step){
        if(step <= 0) return Clamp(value, min, max);

        var steps   = (int) Math.Round((value - min) / (double) step);
        var snapped = min + steps * step;

        return Clamp(snapped, min, max);
    }

    private static long Quantize(long value, long min, long max, long step){
        if(step <= 0L) return Clamp(value, min, max);

        var steps   = (long) Math.Round((value - min) / (double) step);
        var snapped = min + steps * step;

        return Clamp(snapped, min, max);
    }

    private static int Clamp(int value, int min, int max){
        if(value < min) return min;

        return value > max ? max : value;
    }

    private static long Clamp(long value, long min, long max){
        if(value < min) return min;

        return value > max ? max : value;
    }

    private static string Format(int value){
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static string Format(long value){
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static int ParseInt(string text){
        return !int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                   ? throw new FormatException(text)
                   : value;
    }

    private static long ParseLong(string text){
        return !long.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                   ? throw new FormatException(text)
                   : value;
    }

    private static void TryCommitIntText(ConfigEntry<int> cfg, ref string text, int min, int max, int step){
        if(string.IsNullOrWhiteSpace(text)
        || !int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var raw)){
            text = Format(Quantize(cfg.Value, min, max, step));

            return;
        }

        var quantized = Quantize(raw, min, max, step);

        cfg.Value = quantized;
        text      = Format(quantized);
    }

    private static void TryCommitLongText(ConfigEntry<long> cfg, ref string text, long min, long max, long step){
        if(string.IsNullOrWhiteSpace(text)
        || !long.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var raw)){
            text = Format(Quantize(cfg.Value, min, max, step));

            return;
        }

        var quantized = Quantize(raw, min, max, step);

        cfg.Value = quantized;
        text      = Format(quantized);
    }

    private static IntFieldState GetIntEditState(int id, int displayValue){
        if(IntEditStates.TryGetValue(id, out var state)) return state;

        state = new IntFieldState{
                                     Text = Format(displayValue)
                                 };

        IntEditStates[id] = state;

        return state;
    }

    private static LongFieldState GetLongEditState(int id, long displayValue){
        if(LongEditStates.TryGetValue(id, out var state)) return state;

        state = new LongFieldState{
                                      Text = Format(displayValue)
                                  };

        LongEditStates[id] = state;

        return state;
    }

    private static Action<ConfigEntryBase> IntSliderDrawer(int step){
        return entry => {
                   var cfg = (ConfigEntry<int>) entry;

                   GetRange(cfg, out var min, out var max);

                   var id          = cfg.Definition.Key.GetHashCode();
                   var controlName = $"Int_{id}";
                   var focused     = GUI.GetNameOfFocusedControl() == controlName;
                   var state       = GetIntEditState(id, Quantize(cfg.Value, min, max, step));

                   if(state.WasFocused && !focused) TryCommitIntText(cfg, ref state.Text, min, max, step);

                   if(Event.current.type == EventType.KeyDown
                   && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                   && focused){
                       TryCommitIntText(cfg, ref state.Text, min, max, step);
                       GUI.FocusControl(null);
                       focused = false;
                   }

                   if(!focused) state.Text = Format(Quantize(cfg.Value, min, max, step));

                   var value = Quantize(cfg.Value, min, max, step);

                   if(value != cfg.Value) cfg.Value = value;

                   GUILayout.BeginHorizontal();

                   var next = (int) GUILayout.HorizontalSlider(value, min, max);
                   next = Quantize(next, min, max, step);

                   if(next != value){
                       cfg.Value = next;

                       if(!focused) state.Text = Format(next);
                   }

                   GUI.SetNextControlName(controlName);
                   state.Text = GUILayout.TextField(state.Text, GUILayout.Width(LabelWidthInt));

                   state.WasFocused = GUI.GetNameOfFocusedControl() == controlName;

                   GUILayout.EndHorizontal();
               };
    }

    private static Action<ConfigEntryBase> LongSliderDrawer(long step){
        return entry => {
                   var cfg = (ConfigEntry<long>) entry;

                   GetRange(cfg, out var min, out var max);

                   var id          = cfg.Definition.Key.GetHashCode();
                   var controlName = $"Long_{id}";
                   var focused     = GUI.GetNameOfFocusedControl() == controlName;
                   var state       = GetLongEditState(id, Quantize(cfg.Value, min, max, step));

                   if(state.WasFocused && !focused) TryCommitLongText(cfg, ref state.Text, min, max, step);

                   if(Event.current.type == EventType.KeyDown
                   && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                   && focused){
                       TryCommitLongText(cfg, ref state.Text, min, max, step);
                       GUI.FocusControl(null);
                       focused = false;
                   }

                   if(!focused) state.Text = Format(Quantize(cfg.Value, min, max, step));

                   var value = Quantize(cfg.Value, min, max, step);

                   if(value != cfg.Value) cfg.Value = value;

                   GUILayout.BeginHorizontal();

                   var next = (long) GUILayout.HorizontalSlider(value, min, max);
                   next = Quantize(next, min, max, step);

                   if(next != value){
                       cfg.Value = next;

                       if(!focused) state.Text = Format(next);
                   }

                   GUI.SetNextControlName(controlName);
                   state.Text = GUILayout.TextField(state.Text, GUILayout.Width(LabelWidthLong));

                   state.WasFocused = GUI.GetNameOfFocusedControl() == controlName;

                   GUILayout.EndHorizontal();
               };
    }

    private sealed class IntFieldState{
        public string Text;
        public bool   WasFocused;
    }

    private sealed class LongFieldState{
        public string Text;
        public bool   WasFocused;
    }
}
