"""
import_deps.py — 将游戏源码依赖文件复制到 mod 目录，供 Godot 编辑器预览 .tscn 场景。
运行此脚本后可在 Godot 中打开 balatro_inspect_screen.tscn 进行可视化对齐。
打包 mod 前应运行 export_deps.py 清理这些文件。
"""

import os
import shutil
import sys

# ── 路径配置 ──────────────────────────────────────────────────
# 脚本所在目录即 tools/，mod 根目录为上级
TOOLS_DIR = os.path.dirname(os.path.abspath(__file__))
MOD_ROOT = os.path.dirname(TOOLS_DIR)

# 游戏源码根目录
GAME_ROOT = r"d:\[Tool] Godot\STS2v0.110.0"

# ── 依赖清单 ──────────────────────────────────────────────────
# (源相对路径, 目标相对路径) — 目标相对于 MOD_ROOT
DEPENDENCIES = [
    # 箭头图标 (balatro_inspect_screen.tscn 引用)
    ("images/packed/common_ui/settings_tiny_left_arrow.png",
     "images/packed/common_ui/settings_tiny_left_arrow.png"),
    ("images/packed/common_ui/settings_tiny_left_arrow.png.import",
     "images/packed/common_ui/settings_tiny_left_arrow.png.import"),
    ("images/packed/common_ui/settings_tiny_right_arrow.png",
     "images/packed/common_ui/settings_tiny_right_arrow.png"),
    ("images/packed/common_ui/settings_tiny_right_arrow.png.import",
     "images/packed/common_ui/settings_tiny_right_arrow.png.import"),

    # 按钮背景 (入口按钮 + 面板按钮)
    ("images/ui/reward_screen/reward_item_button.png",
     "images/ui/reward_screen/reward_item_button.png"),
    ("images/ui/reward_screen/reward_item_button.png.import",
     "images/ui/reward_screen/reward_item_button.png.import"),

    # HSV 着色器 (箭头 + 按钮金色调)
    ("shaders/hsv.gdshader",
     "shaders/hsv.gdshader"),

    # 粗体字体系列 (被 exclude_filter 排除，本地副本供编辑器预览)
    ("themes/kreon_bold_glyph_space_one.tres",
     "themes/kreon_bold_glyph_space_one.tres"),
    ("themes/kreon_bold_shared.tres",
     "themes/kreon_bold_shared.tres"),
    ("fonts/kreon_bold.ttf",
     "fonts/kreon_bold.ttf"),

    # tickbox 场景 (tscn 中 instance=ExtResource("7_tickbox") 引用)
    ("scenes/ui/tickbox.tscn",
     "scenes/ui/tickbox.tscn"),

    # 音量滑块场景 (面板滑块)
    ("scenes/ui/volume_slider.tscn",
     "scenes/ui/volume_slider.tscn"),

    # 滑块依赖纹理
    ("images/ui/combat/health_bar_bg.png",
     "images/ui/combat/health_bar_bg.png"),
    ("images/ui/combat/health_bar.png",
     "images/ui/combat/health_bar.png"),
    ("images/atlases/ui_atlas.sprites/scrollbar_train_large.tres",
     "images/atlases/ui_atlas.sprites/scrollbar_train_large.tres"),

    # tickbox 勾选/取消贴图
    ("images/atlases/ui_atlas.sprites/checkbox_ticked.tres",
     "images/atlases/ui_atlas.sprites/checkbox_ticked.tres"),
    ("images/atlases/ui_atlas.sprites/checkbox_unticked.tres",
     "images/atlases/ui_atlas.sprites/checkbox_unticked.tres"),

    # 图集 PNG (checkbox.tres 引用的底层纹理)
    ("images/atlases/ui_atlas_0.png.import",
     "images/atlases/ui_atlas_0.png.import"),
    ("images/atlases/ui_atlas_1.png.import",
     "images/atlases/ui_atlas_1.png.import"),

    # NButton 脚本 (tscn 场景引用，必须存在以解析脚本)
    ("src/Core/Nodes/GodotExtensions/NButton.cs",
     "src/Core/Nodes/GodotExtensions/NButton.cs"),
    ("src/Core/Nodes/GodotExtensions/NButton.cs.uid",
     "src/Core/Nodes/GodotExtensions/NButton.cs.uid"),

    # NSlider 脚本 (volume_slider.tscn 引用)
    ("src/Core/Nodes/GodotExtensions/NSlider.cs",
     "src/Core/Nodes/GodotExtensions/NSlider.cs"),
    ("src/Core/Nodes/GodotExtensions/NSlider.cs.uid",
     "src/Core/Nodes/GodotExtensions/NSlider.cs.uid"),
    # 注意: card.tscn 不复制！它在 C# 代码中运行时加载(游戏内置)，复制会污染 PCK
]


def main():
    copied = 0
    skipped = 0
    errors = 0

    for src_rel, dst_rel in DEPENDENCIES:
        src = os.path.join(GAME_ROOT, src_rel)
        dst = os.path.join(MOD_ROOT, dst_rel)

        if not os.path.exists(src):
            print(f"[SKIP]  源不存在: {src_rel}")
            skipped += 1
            continue

        os.makedirs(os.path.dirname(dst), exist_ok=True)

        try:
            shutil.copy2(src, dst)
            copied += 1
        except OSError as e:
            print(f"  [ERROR] {src_rel}: {e}")
            errors += 1

    if skipped > 0 or errors > 0:
        print(f"导入完成: {copied} 复制, {skipped} 跳过, {errors} 失败")
    return 0 if errors == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
