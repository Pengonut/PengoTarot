# fix_sub_tres.py
import os
import re
import shutil

MOD_ROOT = os.path.dirname(os.path.abspath(__file__))

TRES_DIR = os.path.join(MOD_ROOT, "images", "atlases", "card_atlas.sprites", "tarot")

# 尝试的 PNG 目录（按顺序查找）
POSSIBLE_PNG_DIRS = [
    os.path.join(MOD_ROOT, "images", "packed", "card_potraits", "tarot"),
    os.path.join(MOD_ROOT, "images", "packed", "card_portraits", "tarot"),
    # 如果有其他可能路径，可在此添加
]

def find_original_png(orig_filename):
    """在候选目录中查找原始 png 文件"""
    for png_dir in POSSIBLE_PNG_DIRS:
        full = os.path.join(png_dir, orig_filename)
        if os.path.isfile(full):
            return full
    return None

def process_sub_tres(tres_path):
    if not tres_path.endswith("_sub.tres"):
        return False

    print(f"\nProcessing: {os.path.basename(tres_path)}")
    with open(tres_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # 查找 ext_resource 中的 path
    match = re.search(r'path="([^"]+)"', content)
    if not match:
        print("  No path found, skipping.")
        return False

    orig_res_path = match.group(1)
    orig_rel = orig_res_path[len("res://"):] if orig_res_path.startswith("res://") else orig_res_path
    orig_filename = os.path.basename(orig_rel)

    # 查找原始 PNG
    orig_file = find_original_png(orig_filename)
    if not orig_file:
        print(f"  Could not find original png: {orig_filename}")
        print(f"  Checked directories: {POSSIBLE_PNG_DIRS}")
        return False

    # 生成新的 sub 文件名
    if "_sub.png" in orig_filename:
        new_name = orig_filename
    else:
        new_name = orig_filename.replace(".png", "_sub.png")
    new_file = os.path.join(os.path.dirname(orig_file), new_name)

    # 复制图片（若已存在则覆盖）
    shutil.copy2(orig_file, new_file)
    print(f"  Copied: {orig_file} -> {new_name}")

    # 更新 .tres 内的路径
    new_res_path = "res://" + os.path.relpath(new_file, MOD_ROOT).replace("\\", "/")
    content = content.replace(orig_res_path, new_res_path)

    with open(tres_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print(f"  Updated .tres reference to: {new_res_path}")
    return True

def main():
    if not os.path.isdir(TRES_DIR):
        print(f"TRES_DIR not found: {TRES_DIR}")
        return

    count = 0
    for fname in sorted(os.listdir(TRES_DIR)):
        if fname.endswith("_sub.tres"):
            full_path = os.path.join(TRES_DIR, fname)
            if process_sub_tres(full_path):
                count += 1

    print(f"\nDone. Processed {count} files.")
    if count == 0:
        print("No files were processed. Please verify the following:")
        print("- Original PNG files exist in one of the checked directories.")
        print("- The directory name is spelled correctly (card_portraits vs card_potraits).")
        print("You can manually add more candidate directories to POSSIBLE_PNG_DIRS in the script.")

if __name__ == "__main__":
    main()