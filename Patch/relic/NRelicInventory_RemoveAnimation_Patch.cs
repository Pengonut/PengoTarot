#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace PengoTarot.Patch.relic;

/// <summary>
/// 原版会直接从本地顶部遗物栏移除节点，这里先复制一份纯视觉节点，
/// 再播放与丢弃药水相近的向上飞出动画。仅影响表现，不延迟或改动遗物移除逻辑。
/// </summary>
[HarmonyPatch(typeof(NRelicInventory), "Remove")]
public static class NRelicInventory_RemoveAnimation_Patch
{
    [HarmonyPrefix]
    private static void Prefix(NRelicInventory __instance, RelicModel relic)
    {
        var holder = __instance.RelicNodes.FirstOrDefault(node => node.Relic.Model == relic);
        if (holder == null || !GodotObject.IsInstanceValid(holder.Relic))
            return;

        var startPosition = holder.Relic.GlobalPosition;
        var startScale = holder.Relic.Scale;
        TaskHelper.RunSafely(AnimateRemovedRelic(relic, startPosition, startScale));
    }

    private static async Task AnimateRemovedRelic(
        RelicModel relic,
        Vector2 startPosition,
        Vector2 startScale)
    {
        var vfxContainer = NRun.Instance?.GlobalUi?.AboveTopBarVfxContainer;
        if (vfxContainer == null || !GodotObject.IsInstanceValid(vfxContainer))
            return;

        var relicImage = NRelic.Create(relic, NRelic.IconSize.Small);
        if (relicImage == null)
            return;

        relicImage.MouseFilter = Control.MouseFilterEnum.Ignore;
        vfxContainer.AddChildSafely(relicImage);
        await relicImage.AwaitProcessFrame();

        if (!GodotObject.IsInstanceValid(relicImage))
            return;

        relicImage.GlobalPosition = startPosition;
        relicImage.Scale = startScale;
        relicImage.PivotOffset = relicImage.Size * 0.5f;

        // 对齐原版 DiscardPotion：0.4 秒向上飞出；同时略微缩小并淡出，
        // 避免遗物在顶部边缘突然消失。
        var tween = relicImage.CreateTween().SetParallel();
        tween.TweenProperty(relicImage, "global_position:y", startPosition.Y - 100f, 0.4f)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Back);
        tween.TweenProperty(relicImage, "scale", startScale * 0.65f, 0.4f)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(relicImage, "modulate:a", 0f, 0.25f)
            .SetDelay(0.15f)
            .SetEase(Tween.EaseType.In);
        tween.TweenCallback(Callable.From(relicImage.QueueFreeSafely)).SetDelay(0.4f);
    }
}
