



#nullable enable

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Random;

namespace PengoTarot.BalatroEffect
{
    public static class EasePatches
    {
        public static void AnimateEase(Control instance, Vector2 targetScale)
        {
            if (!Config.GlobalDynamicEffect) return;
            const double duration = 0.12f;
            var trv = Traverse.Create(instance);
            var hoverTweenField = trv.Field<Tween>("_hoverTween");
            hoverTweenField.Value?.Kill();
            instance.Scale *= 0.88f;
            instance.Rotation = 0.35f * (Rng.Chaotic.NextBool() ? 1f : -1f);
            var rotTween = instance.CreateTween();
            rotTween
                .TweenProperty(instance, "rotation", 0f, duration)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            var tween = instance.CreateTween();
            tween
                .TweenProperty(instance, "scale", targetScale, duration)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            hoverTweenField.Value = tween;
        }

        static readonly MethodInfo getHoverScale = AccessTools.PropertyGetter(
            typeof(NCardHolder),
            "HoverScale"
        );
        static readonly MethodInfo createHoverTipsMethod = AccessTools.Method(
            typeof(NCardHolder),
            "CreateHoverTips"
        );
        static readonly MethodInfo animateEaseMethod = AccessTools.Method(
            typeof(EasePatches),
            nameof(AnimateEase)
        );

        [HarmonyPatch(typeof(NCardHolder), "DoCardHoverEffects")]
        static class NCardHolderEase
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(
                IEnumerable<CodeInstruction> instructions
            )
            {
                var codes = new CodeMatcher(instructions);


                codes.Start().MatchStartForward(new CodeMatch(i => i.Calls(createHoverTipsMethod)));
                if (codes.IsValid)
                {
                    codes.InsertAfter(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Callvirt, getHoverScale),
                        new CodeInstruction(OpCodes.Call, animateEaseMethod)
                    );
                }
                else
                {
                    GD.PrintErr("PengoTarot: Failed to patch DoCardHoverEffects in NCardHolder");
                }

                return codes.InstructionEnumeration();
            }
        }

        [HarmonyPatch(typeof(NCard), nameof(NCard.OnFreedToPool))]
        static class NCard_OnFreedToPool_Cleanup
        {
            static void Postfix(NCard __instance)
            {
                ShaderController.CleanupCard(__instance);
            }
        }

        [HarmonyPatch(typeof(NCard), nameof(NCard.OnReturnedFromPool))]
        static class NCard_OnReturnedFromPool_Reapply
        {
            static void Postfix(NCard __instance)
            {
                // OnReturnedFromPool 会清空插画材质（_portrait.Material = null），且不触发
                // UpdateVisuals 的 Postfix。轮询等待 Model/Body 就绪后重应用，确保插画 shader 恢复。
                if (__instance == null || !GodotObject.IsInstanceValid(__instance)) return;
                ShaderController.ScheduleReapply(__instance);
            }
        }

        [HarmonyPatch(typeof(NCardHolder), nameof(NCardHolder.ReassignToCard))]
        static class NCardHolder_ReassignToCard_Patch
        {
            static void Postfix(NCardHolder __instance)
            {
                if (__instance.CardNode is NCard card && card.Model != null)
                {
                    ShaderController.ApplyShader(card);
                }
            }
        }

        [HarmonyPatch(typeof(NHandCardHolder), "DoCardHoverEffects")]
        static class NHandCardHolderEase
        {
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(
                IEnumerable<CodeInstruction> instructions
            )
            {
                var codes = new CodeMatcher(instructions);

                codes.Start().MatchStartForward(new CodeMatch(i => i.Calls(createHoverTipsMethod)));
                if (codes.IsValid)
                {
                    codes.InsertAfter(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Callvirt, getHoverScale),
                        new CodeInstruction(OpCodes.Call, animateEaseMethod)
                    );
                }
                else
                {
                    GD.PrintErr("PengoTarot: Failed to patch DoCardHoverEffects in NHandCardHolder");
                }

                return codes.InstructionEnumeration();
            }
        }

        [HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
        public static class NCard_UpdateVisuals_ApplyShader_Patch
        {
            static void Postfix(NCard __instance)
            {
                if (__instance?.Model == null) return;
                if (__instance.Body == null)
                {
                    // Body 未就绪（初始化时序）：轮询等待 Body 就绪后再应用
                    ShaderController.ScheduleReapply(__instance);
                    return;
                }
                ShaderController.ApplyShader(__instance);
            }
        }

        /// <summary>
        /// 原版 inspect 点「查看升级」/切回未升级时，_card.Model 会被替换成 clone（NCard.Model setter
        /// 内 Reload() 会把 _portrait.Material 清为 null）。特效恢复原本依赖 ModelChanged 与
        /// UpdateVisuals 的 Postfix 两条隐式路径，但升级预览路径（ShowUpgradePreview → UpdateVisuals(Upgrade)）
        /// 一旦抛异常或时序错位，插画特效就会丢失且切回也不恢复。
        /// 这里在切换完成后强制重应用特效（与自建屏 NBalatroInspectScreen.SetCard 末尾的显式
        /// ApplyShader 兜底一致）。
        /// </summary>
        [HarmonyPatch(typeof(NInspectCardScreen), "UpdateCardDisplay")]
        static class NInspectCardScreen_UpdateCardDisplay_Reapply
        {
            static void Postfix(NInspectCardScreen __instance)
            {
                var card = AccessTools.Field(typeof(NInspectCardScreen), "_card")?.GetValue(__instance) as NCard;
                if (card == null || !GodotObject.IsInstanceValid(card) || card.Model == null || card.Body == null) return;
                ShaderController.ApplyShader(card);
            }
        }
    }
}