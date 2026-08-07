"""
export_deps.py — 清理从游戏源码复制来的依赖文件。
在打包 mod (.pck) 之前运行，避免将游戏源码文件打包进 mod。
"""

import os
import sys

# ── 路径配置 ──────────────────────────────────────────────────
TOOLS_DIR = os.path.dirname(os.path.abspath(__file__))
MOD_ROOT = os.path.dirname(TOOLS_DIR)

# ── 要清理的文件列表 (相对于 MOD_ROOT) ───────────────────────
CLEANUP_FILES = [
    # 箭头图标
    "images/packed/common_ui/settings_tiny_left_arrow.png",
    "images/packed/common_ui/settings_tiny_left_arrow.png.import",
    "images/packed/common_ui/settings_tiny_right_arrow.png",
    "images/packed/common_ui/settings_tiny_right_arrow.png.import",

    # 按钮背景
    "images/ui/reward_screen/reward_item_button.png",
    "images/ui/reward_screen/reward_item_button.png.import",

    # HSV 着色器
    "shaders/hsv.gdshader",

    # Deps (game source files)
    "src/Core/Nodes/GodotExtensions/NButton.cs",
    "src/Core/Nodes/GodotExtensions/NButton.cs.uid",
]

# 额外要删除的空目录
CLEANUP_DIRS = [
    "images/packed/common_ui",
    "images/packed",
    "images/ui/reward_screen",
    "images/ui",
    "src/Core/Nodes/GodotExtensions",
    "src/Core/Nodes",
    "src/Core",
    "src",
]


def main():
    removed = 0
    skipped = 0
    errors = 0

    for rel in CLEANUP_FILES:
        path = os.path.join(MOD_ROOT, rel)
        if os.path.isfile(path):
            try:
                os.remove(path)
                removed += 1
            except OSError as e:
                print(f"  [ERROR] {rel}: {e}")
                errors += 1
        else:
            skipped += 1

    # 清理空目录
    for rel in CLEANUP_DIRS:
        path = os.path.join(MOD_ROOT, rel)
        try:
            if os.path.isdir(path) and not os.listdir(path):
                os.rmdir(path)
        except OSError:
            pass

    if skipped > 0 or errors > 0:
        print(f"清理完成: {removed} 删除, {skipped} 跳过, {errors} 失败")
    return 0 if errors == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
