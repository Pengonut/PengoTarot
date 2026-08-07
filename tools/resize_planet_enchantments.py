"""
将 images/enchantments/ 下的 12 个星球附魔图标替换为 64×64 尺寸。
拷贝已有的 sample.png (64×64 空白) 覆盖每个 planet_*_enchantment.png。

用法：
  python tools/resize_planet_enchantments.py
"""

import os
import shutil

PROJECT_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ENCH_DIR     = os.path.join(PROJECT_ROOT, "images", "enchantments")
SAMPLE       = os.path.join(ENCH_DIR, "sample.png")

PLANET_NAMES = [
    "mercury", "venus", "earth", "mars",
    "jupiter", "saturn", "uranus", "neptune",
    "pluto", "x", "ceres", "eris",
]

def main():
    if not os.path.exists(SAMPLE):
        print(f"[!] 未找到 sample.png ({SAMPLE})")
        return

    ok = 0
    for name in PLANET_NAMES:
        dst = os.path.join(ENCH_DIR, f"planet_{name}_enchantment.png")
        shutil.copy2(SAMPLE, dst)
        print(f"  [✔] planet_{name}_enchantment.png → 64×64")
        ok += 1

    print(f"\n完成！已更新 {ok} 个附魔图标。")

if __name__ == "__main__":
    main()
