"""Fix missing uid in tarot card_atlas .tres files.

Reads each .tres in card_atlas.sprites/tarot/, checks if ext_resource has a uid,
and fills it from the corresponding .png.import file if missing.
"""
import os
import re
import random
import string

TRES_DIR = r"d:\[Tool] Godot\STS2Mod\PengoTarot\images\atlases\card_atlas.sprites\tarot"
IMPORT_DIR = r"d:\[Tool] Godot\STS2Mod\PengoTarot\images\packed\card_portraits\tarot"


def generate_uid():
    """Generate a Godot-format uid://xxxxxxxxxxxx (random 12-char base64-like)."""
    chars = string.ascii_letters + string.digits
    suffix = ''.join(random.choices(chars, k=12))
    return f"uid://{suffix}"


def get_png_uid(png_name):
    """Read the uid from a .png.import file."""
    import_path = os.path.join(IMPORT_DIR, f"{png_name}.import")
    if not os.path.exists(import_path):
        return None
    with open(import_path, 'r', encoding='utf-8') as f:
        content = f.read()
    m = re.search(r'uid="(uid://[^"]+)"', content)
    return m.group(1) if m else None


def fix_tres(filepath):
    """Fix a single .tres file: add uid to ext_resource if missing."""
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    modified = False
    for i, line in enumerate(lines):
        # Only process ext_resource lines
        if not line.startswith('[ext_resource '):
            continue

        # Already has uid
        if 'uid="uid://' in line:
            break

        # Extract path
        m = re.search(r'path="res://images/packed/card_p[or]rtraits/tarot/([^"]+\.png)"', line)
        if not m:
            # Try broader match
            m = re.search(r'path="res://images/packed/([^"]+)"', line)
            if not m:
                print(f"  SKIP: cannot parse path from line: {line.strip()}")
                return False

        png_filename = os.path.basename(m.group(1))
        png_basename = png_filename.replace('.png', '')

        # Get uid from .import
        uid = get_png_uid(png_basename)
        if not uid:
            uid = generate_uid()
            print(f"  WARN: no .import for {png_filename}, using random {uid}")
        else:
            print(f"  Using uid from .import: {uid}")

        # Insert uid into line
        lines[i] = re.sub(
            r'\[ext_resource type="Texture2D"',
            f'[ext_resource type="Texture2D" uid="{uid}"',
            line
        )
        modified = True
        break

    if not modified:
        return False

    with open(filepath, 'w', encoding='utf-8') as f:
        f.writelines(lines)
    return True

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

    return True


def main():
    fixed = 0
    skipped = 0
    for filename in sorted(os.listdir(TRES_DIR)):
        if not filename.endswith('.tres'):
            continue
        filepath = os.path.join(TRES_DIR, filename)
        print(f"{filename}: ", end='')
        if fix_tres(filepath):
            print("  FIXED")
            fixed += 1
        else:
            print("  OK (already has uid)")
            skipped += 1

    print(f"\nDone. Fixed: {fixed}, Already OK: {skipped}")


if __name__ == '__main__':
    main()
