



#nullable enable

using System.Collections.Generic;
using Godot;

namespace PengoTarot.BalatroEffect
{
    public static class EffectRegistry
    {
        public record EffectDef(int Mode, Shader? Shader, string LocKey, bool Aligns = false);

        private static readonly List<EffectDef> _allEffects = new();
        private static readonly Dictionary<int, EffectDef> _byMode = new();

        public static IReadOnlyList<EffectDef> AllEffects => _allEffects;
        public static IReadOnlyDictionary<int, EffectDef> ByMode => _byMode;

        public static Shader? GetShader(int mode) => _byMode.TryGetValue(mode, out var def) ? def.Shader : null;

        public static void Initialize()
        {
            if (_allEffects.Count > 0) return; 

            
            Register(0, null, "OPTION_NONE");
            Register(1, LoadShader("res://balatroeffect/Shaders/balatro_effects_parts.gdshader"), "OPTION_FOIL");
            Register(2, LoadShader("res://balatroeffect/Shaders/balatro_effects_parts.gdshader"), "OPTION_FOIL_ALT");
            Register(3, LoadShader("res://balatroeffect/Shaders/balatro_effects_parts.gdshader"), "OPTION_POLYCHROME");
            Register(4, LoadShader("res://balatroeffect/Shaders/balatro_effects_parts.gdshader"), "OPTION_HOLOGRAPHIC");
            Register(5, LoadShader("res://balatroeffect/Shaders/balatro_effects_parts.gdshader"), "OPTION_NEGATIVE");       // 负片-A（mode5：原 shader 复杂负片，带蓝光泽）
            Register(6, LoadShader("res://balatroeffect/Shaders/balatro_effects_parts.gdshader"), "OPTION_NEGATIVE_BLUE"); // 负片-B（mode6：简单反色，原 NegativeShaderPatch 效果）
            Register(7, LoadShader("res://balatroeffect/Shaders/pengo_aniso_rainbow.gdshader"), "OPTION_ANISO_FIXED", aligns: true);
            
            Register(8, LoadShader("res://balatroeffect/Shaders/pengo_aniso_rainbow.gdshader"), "OPTION_ANISO_STRIPE", aligns: true);
            Register(9, LoadShader("res://balatroeffect/Shaders/pengo_aniso_rainbow.gdshader"), "OPTION_ANISO_DUAL", aligns: true);
            Register(10, LoadShader("res://balatroeffect/Shaders/pengo_vhs.gdshader"), "OPTION_VHS", aligns: true);
            Register(11, LoadShader("res://balatroeffect/Shaders/pengo_crt.gdshader"), "OPTION_CRT", aligns: false);
            Register(12, LoadShader("res://balatroeffect/Shaders/pengo_vhs2.gdshader"), "OPTION_VHS2", aligns: false);
            Register(13, LoadShader("res://balatroeffect/Shaders/pengo_sweep.gdshader"), "OPTION_SWEEP", aligns: false);
            
            Register(15, LoadShader("res://balatroeffect/Shaders/pengo_hover_glow.gdshader"), "OPTION_HOVER_GLOW", aligns: true);
            Register(16, LoadShader("res://balatroeffect/Shaders/pengo_glitter.gdshader"), "OPTION_GLITTER", aligns: true);
            Register(17, LoadShader("res://balatroeffect/Shaders/pengo_aurora_overlay.gdshader"), "OPTION_AURORA", aligns: true);
            Register(18, LoadShader("res://balatroeffect/Shaders/pengo_pixelate.gdshader"), "OPTION_PIXELATE", aligns: false);
            Register(19, LoadShader("res://balatroeffect/Shaders/pengo_outline.gdshader"), "OPTION_OUTLINE", aligns: false);
            Register(20, LoadShader("res://balatroeffect/Shaders/pengo_starcloud.gdshader"), "OPTION_STARCLOUD", aligns: true);
            Register(21, LoadShader("res://balatroeffect/Shaders/pengo_randomstars.gdshader"), "OPTION_RANDOMSTARS", aligns: true);

        }

        private static void Register(int mode, Shader? shader, string locKey, bool aligns = false)
        {
            var def = new EffectDef(mode, shader, locKey, aligns);
            _allEffects.Add(def);
            _byMode[mode] = def;
        }

        private static Shader? LoadShader(string path)
        {
            if (Godot.FileAccess.FileExists(path))
                return GD.Load<Shader>(path);
            GD.PrintErr($"[EffectRegistry] Shader not found: {path}");
            return null;
        }
    }
}