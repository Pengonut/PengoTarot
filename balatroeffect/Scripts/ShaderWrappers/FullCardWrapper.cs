// PengoTarot: FullCardEffectContainer (double SubViewport for full-card shader effects).

#nullable enable

using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace PengoTarot.BalatroEffect
{
    public static partial class FullCardWrapper
    {
        public const string FxName = "BalatroFullCardFx";

        public static FullCardEffectContainer CreateFullCard(NCard card, string cid, TiltWrapper.TiltContainer tilt, int mode)
        {
            Shader? sh = EffectRegistry.GetShader(mode) ?? EffectRegistry.GetShader(1);
            var fm = new ShaderMaterial { Shader = sh };
            fm.SetShaderParameter(TiltWrapper.SeedKey, card.GetHashCode() % 10000 / 10.0f);
            // EffectRegistry.ApplyNoiseTexture(fm); // TODO: 噪声纹理功能未完成，暂时注释

            var fc = new FullCardEffectContainer
            {
                Name = FxName, Material = fm, Size = new Vector2I(1200, 1200), Stretch = true,
                MouseFilter = Control.MouseFilterEnum.Ignore, Position = new Vector2(-600, -600),
                PivotOffset = new Vector2(600, 600), CardId = cid
            };
            var vp = new SubViewport { Name = "SubViewport", TransparentBg = true, Disable3D = true, Size = new Vector2I(1200, 1200) };
            tilt.GetParent()?.RemoveChild(tilt); tilt.Position = Vector2.Zero;
            vp.AddChild(tilt);
            fc.AddChild(vp);

            var root = (Control)vp.GetChild(0); // tilt's root

            //GD.Print($"[DBG] CreateFullCard before WrapParts, root children: {string.Join(", ", root.GetChildren().Select(c => c.Name))}");

            PartWrapper.WrapParts(cid, root);

            //GD.Print($"[DBG] CreateFullCard after WrapParts, root children: {string.Join(", ", root.GetChildren().Select(c => c.Name))}");

            // Map card region in 1200×1200 texture (UV 0.5→0.9) to the same
            // 480-based UV space used by per-part effects on Body
            // (WrapPartsInspect: global_uv ∈ 0.5→1.5 for modes 1-4, 0→1 for mode 5).
            // uv_scale = (1.5-0.5)/(0.9-0.5) = 2.5;  uv_offset = 0.5 - 0.5*2.5 = -0.75
            // foil_alt: 0.5*2.5 + x = 0 → x = -1.25
            fm.SetShaderParameter(TiltWrapper.EffectModeKey, mode);
            fm.SetShaderParameter(TiltWrapper.IntensityKey, Config.GetIntensity(cid));
            fm.SetShaderParameter(TiltWrapper.UvOffsetKey, new Vector2(-0.75f, -0.75f));
            fm.SetShaderParameter(TiltWrapper.UvScaleKey, new Vector2(2.5f, 2.5f));
            fm.SetShaderParameter(TiltWrapper.FoilAltUvOffsetKey, new Vector2(-1.25f, -1.25f));

            return fc;
        }

        // ========================================================================
        // FullCardEffectContainer — dynamic effect switching + tilt shimmer
        // ========================================================================
        public partial class FullCardEffectContainer : SubViewportContainer
        {
            public string? CardId;
            private TiltWrapper.TiltContainer? _tilt;

            public override void _Process(double _)
            {
                if (Material is not ShaderMaterial m || string.IsNullOrEmpty(CardId)) return;
                if (_tilt == null || !GodotObject.IsInstanceValid(_tilt))
                {
                    var vp = GetNodeOrNull<SubViewport>("SubViewport");
                    if (vp?.GetChildCount() > 0) _tilt = vp.GetChild(0) as TiltWrapper.TiltContainer;
                }

                int e = Config.GetEffect(CardId, "FullCard");
                if (e == 0) { m.SetShaderParameter(TiltWrapper.EffectModeKey, 0); return; }

                Shader? ts = EffectRegistry.GetShader(e);
                if (ts != null && m.Shader != ts)
                {
                    var nm = new ShaderMaterial { Shader = ts };
                    nm.SetShaderParameter(TiltWrapper.SeedKey, m.GetShaderParameter(TiltWrapper.SeedKey));
                    // EffectRegistry.ApplyNoiseTexture(nm); // TODO: 噪声纹理功能未完成，暂时注释
                    nm.SetShaderParameter(TiltWrapper.UvOffsetKey, m.GetShaderParameter(TiltWrapper.UvOffsetKey));
                    nm.SetShaderParameter(TiltWrapper.UvScaleKey, m.GetShaderParameter(TiltWrapper.UvScaleKey));
                    Material = nm; m = nm;
                }
                m.SetShaderParameter(TiltWrapper.EffectModeKey, e);
                m.SetShaderParameter(TiltWrapper.IntensityKey, Config.GetIntensity(CardId));

                // Card-play rotation animation takes priority over tilt shimmer
                var playRot = CardPlayTracker.GetPlayRotation(CardId);
                if (playRot.HasValue)
                {
                    m.SetShaderParameter(TiltWrapper.XRotKey, playRot.Value.xRot);
                    m.SetShaderParameter(TiltWrapper.YRotKey, playRot.Value.yRot);
                }
                else if (_tilt?.Material is ShaderMaterial tm && GodotObject.IsInstanceValid(_tilt))
                { m.SetShaderParameter(TiltWrapper.XRotKey, tm.GetShaderParameter(TiltWrapper.XRotKey)); m.SetShaderParameter(TiltWrapper.YRotKey, tm.GetShaderParameter(TiltWrapper.YRotKey)); }
            }
        }
    }
}
