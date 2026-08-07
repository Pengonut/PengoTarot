"""
将 images/packed/card_portraits/planet/ 下的 WebP 转为 PNG，
尺寸从 142×190 放大到 568×760 (4x)，使用最近邻插值保留像素画质。

用法：
  python convert_planet_webp_to_png.py
"""

import os
import re
from PIL import Image

PROJECT_ROOT = os.path.dirname(os.path.abspath(__file__))
PLANET_DIR   = os.path.join(PROJECT_ROOT, "images", "packed", "card_portraits", "planet")

def webp_to_png_name(webp_name: str) -> str:
    """
    将 WebP 文件名映射为 planet_{name}.png。
    例如:
      Mercury.webp  → planet_mercury.png
      Planet_X.webp → planet_x.png
      Earth.webp    → planet_earth.png
    """
    base = os.path.splitext(webp_name)[0]      # 去掉 .webp
    # 统一转小写后，去掉可能存在的 "planet_" 前缀
    name = base.lower().removeprefix("planet_")
    return f"planet_{name}.png"

def main():
    # 扫描目录下所有 .webp 文件
    webp_files = [f for f in os.listdir(PLANET_DIR) if f.lower().endswith(".webp")]
    if not webp_files:
        print("  [!] 未找到任何 .webp 文件")
        return

    webp_files.sort()
    success = 0
    errors  = 0

    for webp_name in webp_files:
        webp_path = os.path.join(PLANET_DIR, webp_name)
        png_name  = webp_to_png_name(webp_name)
        png_path  = os.path.join(PLANET_DIR, png_name)

        try:
            img = Image.open(webp_path)
            # 最近邻插值放大 4x (142→568, 190→760)
            img = img.resize((568, 760), Image.NEAREST)
            img.save(png_path, "PNG")
            print(f"  [✔] {webp_name} → {png_name}  (142×190 → 568×760, NEAREST)")
            success += 1
        except Exception as e:
            print(f"  [✘] {webp_name} 转换失败: {e}")
            errors += 1

    print()
    print(f"完成：成功 {success}，失败 {errors}")

if __name__ == "__main__":
    main()
