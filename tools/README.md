# PengoTarot 工具脚本合集

> 所有脚本通过 `python tools/<脚本名>` 从项目根目录运行。

---

## 卡牌资源生成

### `generate_planet_cards.py`
批量生成 12 张星球牌的初始资源文件（仅首次搭建时使用）。

| 生成内容 | 路径 |
|---|---|
| `.tres` (AtlasTexture) | `images/atlases/card_atlas.sprites/planet/` |
| 卡面 `.png` (空白) | `images/packed/card_portraits/planet/` |
| 附魔图标 `.png` (空白) | `images/enchantments/` |

**注意：** 已存在的文件会自动跳过，可重复运行。

---

## 图片格式转换

### `convert_planet_webp_to_png.py`
将 `images/packed/card_portraits/planet/` 下的 WebP 图片转为 PNG，
尺寸从 142×190 → 568×760 (4x)，最近邻插值保留像素画质。

**用法：**
```powershell
pip install Pillow   # 首次需要
python tools/convert_planet_webp_to_png.py
```

---

## .tres UID 修复

### `fix_tres_uids.py`
为项目中的 `.tres` 文件重新生成或修复 Godot UID。

**用法：**
```powershell
python tools/fix_tres_uids.py
```

---

## 附魔图标

### `resize_planet_enchantments.py`
将 12 个星球附魔图标替换为 64×64 尺寸（拷贝 `sample.png`）。

**用法：**
```powershell
python tools/resize_planet_enchantments.py
```

---

### `fix_tres_path.py`
修复 `.tres` 文件中 `ext_resource` 的路径引用。

**用法：**
```powershell
python tools/fix_tres_path.py
```

---

## 其他

### `fix.py`
其他杂项修复。

### `pth.py`
路径相关的辅助工具。

### `convert_planet_webp_to_png.py`
将 `images/packed/card_portraits/planet/` 下的 WebP 原画转为 PNG，
并 4x 放大（142×190 → 568×760），使用最近邻插值保留像素画质。

**依赖：** `pip install Pillow`

**行为：** 自动扫描目录下所有 `.webp` 文件，文件名大小写不敏感。
`Planet_X.webp` → `planet_x.png`，`Mercury.webp` → `planet_mercury.png` 等。

---

## .tres 文件修复

### `fix_tres_path.py`
修复早期 typo：将 `.tres` 中错误的 `card_potraits` 路径修正为 `card_portraits`。

### `fix_tres_uids.py`
为 `.tres` 文件中缺失 uid 的 `ext_resource` 行补充 uid，从对应的 `.png.import` 文件中读取。

### `fix.py` (原 `fix_sub_tres.py`)
处理 `_sub.tres` 的子卡文件，修复或补充相关资源引用。

---

## 代码生成

### `pth.py`
生成 12 张星球牌的附魔 C# 类代码（`PlanetXxxEnchantment.cs`），
输出到 `src/Core/Models/Enchantments/`。一次性搭建工具。
