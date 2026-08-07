"""Fix typo in tarot card_atlas .tres files: card_potraits -> card_portraits."""
import os

TRES_DIR = r"d:\[Tool] Godot\STS2Mod\PengoTarot\images\atlases\card_atlas.sprites\tarot"

fixed = 0
for filename in sorted(os.listdir(TRES_DIR)):
    if not filename.endswith('.tres'):
        continue
    filepath = os.path.join(TRES_DIR, filename)
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    if 'card_potraits' not in content:
        continue
    
    new_content = content.replace('card_potraits', 'card_portraits')
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(new_content)
    print(f"FIXED: {filename}")
    fixed += 1

print(f"\nTotal fixed: {fixed}")
