
#nullable enable

using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using PengoTarot.Data;

namespace PengoTarot.Patches
{
    /*
     * 已内置为作者预设（全局预设，2026-08-02）：
     *   - 负片塔罗卡 → res://balatroeffect/Assets/author_preset.json（Portrait = mode 2 负片）
     *   - 负片附魔 → res://balatroeffect/Assets/author_enchant_preset.json（Portrait = mode 2 负片）
     * 由 Config.ApplyAllAuthorPresets 在首次初始化 / 版本升级时按 id 合并应用。
     * 此补丁保留作参考，暂不启用。
    [HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals), 
        new[] { typeof(PileType), typeof(CardPreviewMode) })]
    [HarmonyPriority(Priority.Low)]
    public static class NegativeShaderPatch
    {
        private static readonly Lazy<ShaderMaterial> _negativeMaterial = new(() =>
        {
            var shader = new Shader();
            shader.Code = @"
                shader_type canvas_item;
                void fragment() {
                    COLOR = vec4(1.0 - COLOR.rgb, COLOR.a);
                }";
            var material = new ShaderMaterial();
            material.Shader = shader;
            return material;
        });

        static void Postfix(NCard __instance)
        {
            if (__instance == null || !GodotObject.IsInstanceValid(__instance))
                return;

            ApplyNegativeShader(__instance);
        }

        private static void ApplyNegativeShader(NCard cardNode)
        {
            var model = cardNode.Model;
            if (model == null) return;

            bool shouldApply = false;

            if (model.Pool is TarotPool && 
                model.Id.Entry.Contains("sub", StringComparison.OrdinalIgnoreCase))
                shouldApply = true;

            if (!shouldApply && model.Enchantment != null &&
                model.Enchantment.Id.Entry.Contains("sub", StringComparison.OrdinalIgnoreCase) &&
                model.Enchantment.GetType().Namespace == "PengoTarot.Enchantments")
                shouldApply = true;

            var portrait = cardNode.FindChild("Portrait", recursive: true, owned: false) as CanvasItem;
            if (portrait != null)
            {
                portrait.Material = shouldApply ? _negativeMaterial.Value : null;
            }
        }
    }
    */
}