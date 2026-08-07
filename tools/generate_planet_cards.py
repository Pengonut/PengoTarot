"""
为 PengoTarot 项目批量生成 12 张星球牌的 .tres / 卡面 PNG / 附魔图标 PNG。

生成的目录结构：
  images/atlases/card_atlas.sprites/planet/planet_{name}.tres     (AtlasTexture 资源)
  images/packed/card_portraits/planet/planet_{name}.png          (卡面占位 PNG)
  images/enchantments/planet_{name}_enchantment.png              (附魔图标占位 PNG)

用法：
  python generate_planet_cards.py
"""

import os
import random
import string

# ---------------------------------------------------------------------------
# 配置
# ---------------------------------------------------------------------------
PROJECT_ROOT = os.path.dirname(os.path.abspath(__file__))

# 12 张星球牌的名称 (与 PlanetDeck.cs 中的 ID 对应)
PLANET_NAMES = [
    "mercury",
    "venus",
    "earth",
    "mars",
    "jupiter",
    "saturn",
    "uranus",
    "neptune",
    "pluto",
    "x",
    "ceres",
    "eris",
]

# 输出目录（相对于项目根目录）
TRES_DIR          = os.path.join(PROJECT_ROOT, "images", "atlases", "card_atlas.sprites", "planet")
PNG_DIR           = os.path.join(PROJECT_ROOT, "images", "packed", "card_portraits", "planet")
ENCHANTMENT_DIR   = os.path.join(PROJECT_ROOT, "images", "enchantments")


# ---------------------------------------------------------------------------
# 工具函数
# ---------------------------------------------------------------------------

def random_uid(length: int = 9) -> str:
    """生成一个类似 Godot UID 的随机字符串 (字母 + 数字)。"""
    chars = string.ascii_lowercase + string.digits
    return "".join(random.choices(chars, k=length))


def random_ext_id(length: int = 5) -> str:
    """生成 ext_resource 的随机 id 后缀。"""
    chars = string.ascii_lowercase + string.digits
    return "".join(random.choices(chars, k=length))


def create_empty_png(filepath: str) -> None:
    """创建一个最小的 1×1 透明 PNG 文件作为占位符。"""
    # 最小有效 PNG: 1x1 像素, 8-bit RGBA, 完全透明
    # PNG signature + IHDR + IDAT + IEND
    png_data = bytes([
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,  # PNG signature
        0x00, 0x00, 0x00, 0x0D,                            # IHDR length
        0x49, 0x48, 0x44, 0x52,                            # IHDR chunk type
        0x00, 0x00, 0x00, 0x01,                            # width = 1
        0x00, 0x00, 0x00, 0x01,                            # height = 1
        0x08, 0x06,                                        # bit depth=8, color type=6 (RGBA)
        0x00, 0x00, 0x00, 0x00,                            # compression, filter, interlace
        0x1F, 0x15, 0xC4, 0x89,                            # IHDR CRC
        0x00, 0x00, 0x00, 0x0C,                            # IDAT length
        0x49, 0x44, 0x41, 0x54,                            # IDAT chunk type
        0x78, 0x9C, 0x63, 0x68, 0x00, 0x00, 0x00, 0x00,  # zlib compressed data
        0x00, 0x01, 0x00, 0x01,                            # (transparent RGBA pixel)
        0x0D, 0x7A, 0x9D, 0xB6,                            # IDAT CRC
        0x00, 0x00, 0x00, 0x00,                            # IEND length
        0x49, 0x45, 0x4E, 0x44,                            # IEND chunk type
        0xAE, 0x42, 0x60, 0x82,                            # IEND CRC
    ])
    with open(filepath, "wb") as f:
        f.write(png_data)


def generate_tres_content(planet_name: str) -> str:
    """生成 Godot AtlasTexture 的 .tres 文件内容。"""
    res_uid    = random_uid(9)   # 资源自身 UID
    tex_uid    = random_uid(11)  # 贴图文件的 UID
    ext_id     = random_ext_id()
    png_path   = f"res://images/packed/card_portraits/planet/planet_{planet_name}.png"

    return (
        f'[gd_resource type="AtlasTexture" load_steps=2 format=3 uid="uid://{res_uid}"]\n'
        f'\n'
        f'[ext_resource type="Texture2D" uid="uid://{tex_uid}" path="{png_path}" id="1_{ext_id}"]\n'
        f'\n'
        f'[resource]\n'
        f'atlas = ExtResource("1_{ext_id}")\n'
    )


# ---------------------------------------------------------------------------
# 主流程
# ---------------------------------------------------------------------------

def main():
    print(f"项目根目录: {PROJECT_ROOT}")
    print()

    # 创建输出目录
    os.makedirs(TRES_DIR, exist_ok=True)
    os.makedirs(PNG_DIR, exist_ok=True)
    os.makedirs(ENCHANTMENT_DIR, exist_ok=True)
    print(f"  [OK] 目录已就绪: {os.path.relpath(TRES_DIR, PROJECT_ROOT)}")
    print(f"  [OK] 目录已就绪: {os.path.relpath(PNG_DIR, PROJECT_ROOT)}")
    print(f"  [OK] 目录已就绪: {os.path.relpath(ENCHANTMENT_DIR, PROJECT_ROOT)}")
    print()

    success_count = 0
    skip_count = 0
    for name in PLANET_NAMES:
        tres_path     = os.path.join(TRES_DIR, f"planet_{name}.tres")
        png_path      = os.path.join(PNG_DIR, f"planet_{name}.png")
        enchant_path  = os.path.join(ENCHANTMENT_DIR, f"planet_{name}_enchantment.png")

        # 所有文件都已存在 → 跳过
        if os.path.exists(tres_path) and os.path.exists(png_path) and os.path.exists(enchant_path):
            print(f"  [-] planet_{name} 已存在，跳过")
            skip_count += 1
            continue

        # 生成 .tres
        content = generate_tres_content(name)
        with open(tres_path, "w", encoding="utf-8") as f:
            f.write(content)

        # 生成空白卡面 .png
        create_empty_png(png_path)

        # 生成空白附魔图标 .png
        create_empty_png(enchant_path)

        print(f"  [✔] planet_{name}.tres  +  planet_{name}.png  +  planet_{name}_enchantment.png")
        success_count += 1

    print()
    if skip_count > 0:
        print(f"跳过 {skip_count} 张已存在的牌，", end="")
    print(f"本次生成 {success_count} 张星球牌资源。")
    print(f"  .tres           → {os.path.relpath(TRES_DIR, PROJECT_ROOT)}/")
    print(f"  卡面 .png       → {os.path.relpath(PNG_DIR, PROJECT_ROOT)}/")
    print(f"  附魔图标 .png   → {os.path.relpath(ENCHANTMENT_DIR, PROJECT_ROOT)}/")
    print()
    print("提示：请用 Photoshop 打开各 .png 文件绘制卡面和附魔图标，完成后重新导入 Godot 即可。")


if __name__ == "__main__":
    main()
